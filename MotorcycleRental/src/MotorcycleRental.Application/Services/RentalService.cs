using FluentValidation;
using Microsoft.Extensions.Logging;
using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Rental;
using MotorcycleRental.Application.Interfaces;
using MotorcycleRental.Application.Mappings;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Enums;
using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateRentalRequest> _createValidator;
        private readonly ILogger<RentalService> _logger;

        public RentalService(
            IUnitOfWork unitOfWork,
            IValidator<CreateRentalRequest> createValidator,
            ILogger<RentalService> logger)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _logger = logger;
        }

        public async Task<ApiResponse<RentalResponse>> CreateAsync(CreateRentalRequest request)
        {
            try
            {
                // Validar request
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<RentalResponse>.FailureResponse(
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // Buscar entregador
                var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
                if (driver == null)
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Driver not found");
                }

                // Verificar se entregador pode alugar
                if (!driver.CanRent())
                {
                    return ApiResponse<RentalResponse>.FailureResponse(
                        "Driver cannot rent. Only drivers with CNH type A or AB are allowed"
                    );
                }

                // Verificar se entregador já tem locação ativa
                if (driver.HasActiveRental())
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Driver already has an active rental");
                }

                // Buscar moto
                var motorcycle = await _unitOfWork.Motorcycles.GetByIdAsync(request.MotorcycleId);
                if (motorcycle == null)
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Motorcycle not found");
                }

                // Verificar se moto está disponível
                if (!motorcycle.IsAvailable())
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Motorcycle is not available");
                }

                // Criar locação
                var rental = new Rental(
                    motorcycle.Id,
                    driver.Id,
                    request.Plan,
                    request.StartDate
                );

                await _unitOfWork.Rentals.AddAsync(rental);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Rental created: {RentalId}", rental.Id);

                return ApiResponse<RentalResponse>.SuccessResponse(
                    rental.ToResponse(),
                    "Rental created successfully"
                );
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return ApiResponse<RentalResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return ApiResponse<RentalResponse>.FailureResponse("An error occurred while creating the rental");
            }
        }

        public async Task<ApiResponse<RentalResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var rental = await _unitOfWork.Rentals.GetByIdAsync(id);

                if (rental == null)
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Rental not found");
                }

                return ApiResponse<RentalResponse>.SuccessResponse(rental.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental by id: {RentalId}", id);
                return ApiResponse<RentalResponse>.FailureResponse("An error occurred while retrieving the rental");
            }
        }

        public async Task<ApiResponse<RentalReturnSimulationResponse>> SimulateReturnAsync(
            Guid id,
            SimulateReturnRequest request)
        {
            try
            {
                var rental = await _unitOfWork.Rentals.GetByIdAsync(id);

                if (rental == null)
                {
                    return ApiResponse<RentalReturnSimulationResponse>.FailureResponse("Rental not found");
                }

                if (rental.Status != RentalStatus.Active)
                {
                    return ApiResponse<RentalReturnSimulationResponse>.FailureResponse("Rental is not active");
                }

                // Simular devolução
                var result = rental.SimulateReturn(request.ReturnDate);

                // Criar mensagem explicativa
                string message;
                if (result.PenaltyAmount > 0)
                {
                    message = $"Early return detected. A penalty of {result.PenaltyAmount:C} will be applied.";
                }
                else if (result.AdditionalAmount > 0)
                {
                    message = $"Late return detected. An additional fee of {result.AdditionalAmount:C} will be applied.";
                }
                else
                {
                    message = "Return on expected date. No additional charges.";
                }

                var response = result.ToSimulationResponse(rental.DailyRate, message);

                return ApiResponse<RentalReturnSimulationResponse>.SuccessResponse(response);
            }
            catch (InvalidDomainOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot simulate return");
                return ApiResponse<RentalReturnSimulationResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating rental return");
                return ApiResponse<RentalReturnSimulationResponse>.FailureResponse(
                    "An error occurred while simulating the return"
                );
            }
        }

        public async Task<ApiResponse<RentalResponse>> ProcessReturnAsync(
            Guid id,
            ProcessReturnRequest request)
        {
            try
            {
                var rental = await _unitOfWork.Rentals.GetByIdAsync(id);

                if (rental == null)
                {
                    return ApiResponse<RentalResponse>.FailureResponse("Rental not found");
                }

                // Processar devolução
                rental.ProcessReturn(request.ReturnDate);

                await _unitOfWork.Rentals.UpdateAsync(rental);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Rental returned: {RentalId}", rental.Id);

                return ApiResponse<RentalResponse>.SuccessResponse(
                    rental.ToResponse(),
                    "Rental returned successfully"
                );
            }
            catch (InvalidDomainOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot process return");
                return ApiResponse<RentalResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rental return");
                return ApiResponse<RentalResponse>.FailureResponse(
                    "An error occurred while processing the return"
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<RentalResponse>>> GetByDriverIdAsync(Guid driverId)
        {
            try
            {
                var rentals = await _unitOfWork.Rentals.GetByDriverIdAsync(driverId);
                var rentalResponses = rentals.Select(r => r.ToResponse());

                return ApiResponse<IEnumerable<RentalResponse>>.SuccessResponse(rentalResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rentals by driver id: {DriverId}", driverId);
                return ApiResponse<IEnumerable<RentalResponse>>.FailureResponse(
                    "An error occurred while retrieving rentals"
                );
            }
        }
    }
}