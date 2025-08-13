using FluentValidation;
using Microsoft.Extensions.Logging;
using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Motorcycle;
using MotorcycleRental.Application.Interfaces;
using MotorcycleRental.Application.Mappings;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Events;
using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Application.Services
{
    public class MotorcycleService : IMotorcycleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IRetryService _retryService;
        private readonly IValidator<CreateMotorcycleRequest> _createValidator;
        private readonly ILogger<MotorcycleService> _logger;

        public MotorcycleService(
            IUnitOfWork unitOfWork,
            IMessagePublisher messagePublisher,
            IRetryService retryService,
            IValidator<CreateMotorcycleRequest> createValidator,
            ILogger<MotorcycleService> logger)
        {
            _unitOfWork = unitOfWork;
            _messagePublisher = messagePublisher;
            _retryService = retryService;
            _createValidator = createValidator;
            _logger = logger;
        }

        public async Task<ApiResponse<MotorcycleResponse>> CreateAsync(CreateMotorcycleRequest request)
        {
            try
            {
                // Validar request
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<MotorcycleResponse>.FailureResponse(
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // USAR RETRY PARA TODAS AS OPERAÇÕES DE BANCO (incluindo validação)
                var motorcycle = await _retryService.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("💾 Tentando validar e salvar moto no banco...");

                    // Verificar se placa já existe (dentro do retry)
                    if (await _unitOfWork.Motorcycles.ExistsByPlateAsync(request.Plate))
                    {
                        throw new BusinessRuleException("PLATE_EXISTS", "Plate already exists");
                    }

                    var moto = new Motorcycle(
                        request.Identifier,
                        request.Year,
                        request.Model,
                        request.Plate
                    );

                    await _unitOfWork.Motorcycles.AddAsync(moto);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("✅ Moto salva com sucesso!");
                    return moto;
                });

                // USAR RETRY PARA PUBLICAR EVENTO
                await _retryService.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("📤 Tentando publicar evento...");

                    var motorcycleEvent = new MotorcycleCreatedEvent(
                        motorcycle.Id,
                        motorcycle.Identifier,
                        motorcycle.Year,
                        motorcycle.Model,
                        motorcycle.Plate.Value
                    );

                    await _messagePublisher.PublishAsync(motorcycleEvent);

                    _logger.LogInformation("✅ Evento publicado com sucesso!");
                });

                _logger.LogInformation("Motorcycle created: {MotorcycleId}", motorcycle.Id);

                return ApiResponse<MotorcycleResponse>.SuccessResponse(
                    motorcycle.ToResponse(),
                    "Motorcycle created successfully"
                );
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return ApiResponse<MotorcycleResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating motorcycle after all retries");
                return ApiResponse<MotorcycleResponse>.FailureResponse(
                    "An error occurred while creating the motorcycle");
            }
        }

        public async Task<ApiResponse<MotorcycleResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var motorcycle = await _unitOfWork.Motorcycles.GetByIdAsync(id);

                if (motorcycle == null)
                {
                    return ApiResponse<MotorcycleResponse>.FailureResponse("Motorcycle not found");
                }

                return ApiResponse<MotorcycleResponse>.SuccessResponse(motorcycle.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motorcycle by id: {MotorcycleId}", id);
                return ApiResponse<MotorcycleResponse>.FailureResponse(
                    "An error occurred while retrieving the motorcycle");
            }
        }

        public async Task<ApiResponse<MotorcycleListResponse>> GetAllAsync(string? plateFilter = null)
        {
            try
            {
                var motorcycles = await _unitOfWork.Motorcycles.GetByPlateFilterAsync(plateFilter);
                var motorcyclesList = motorcycles.ToList();

                var response = new MotorcycleListResponse(
                    motorcyclesList.ToResponse(),
                    motorcyclesList.Count
                );

                return ApiResponse<MotorcycleListResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motorcycles");
                return ApiResponse<MotorcycleListResponse>.FailureResponse(
                    "An error occurred while retrieving motorcycles");
            }
        }

        public async Task<ApiResponse<MotorcycleResponse>> UpdatePlateAsync(Guid id, UpdateMotorcyclePlateRequest request)
        {
            try
            {
                var motorcycle = await _unitOfWork.Motorcycles.GetByIdAsync(id);

                if (motorcycle == null)
                {
                    return ApiResponse<MotorcycleResponse>.FailureResponse("Motorcycle not found");
                }

                // Verificar se nova placa já existe
                if (await _unitOfWork.Motorcycles.ExistsByPlateAsync(request.Plate))
                {
                    return ApiResponse<MotorcycleResponse>.FailureResponse("Plate already exists");
                }

                // USAR RETRY PARA ATUALIZAÇÃO
                await _retryService.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("🔄 Tentando atualizar placa da moto...");

                    // Atualizar placa
                    motorcycle.UpdatePlate(request.Plate);

                    await _unitOfWork.Motorcycles.UpdateAsync(motorcycle);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("✅ Placa atualizada com sucesso!");
                });

                _logger.LogInformation("Motorcycle plate updated: {MotorcycleId}", id);

                return ApiResponse<MotorcycleResponse>.SuccessResponse(
                    motorcycle.ToResponse(),
                    "Plate updated successfully"
                );
            }
            catch (InvalidDomainOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot update motorcycle plate");
                return ApiResponse<MotorcycleResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating motorcycle plate after retries");
                return ApiResponse<MotorcycleResponse>.FailureResponse(
                    "An error occurred while updating the plate");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var motorcycle = await _unitOfWork.Motorcycles.GetByIdAsync(id);

                if (motorcycle == null)
                {
                    return ApiResponse<bool>.FailureResponse("Motorcycle not found");
                }

                // Verificar se pode deletar
                if (!motorcycle.CanBeDeleted())
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Motorcycle cannot be deleted because it has rental records");
                }

                // USAR RETRY PARA EXCLUSÃO
                await _retryService.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("🗑️ Tentando deletar moto...");

                    await _unitOfWork.Motorcycles.DeleteAsync(motorcycle);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("✅ Moto deletada com sucesso!");
                });

                _logger.LogInformation("Motorcycle deleted: {MotorcycleId}", id);

                return ApiResponse<bool>.SuccessResponse(true, "Motorcycle deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting motorcycle after retries");
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while deleting the motorcycle");
            }
        }
    }
}