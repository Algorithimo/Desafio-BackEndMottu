using FluentValidation;
using Microsoft.Extensions.Logging;
using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Driver;
using MotorcycleRental.Application.Interfaces;
using MotorcycleRental.Application.Mappings;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Application.Services
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IValidator<CreateDriverRequest> _createValidator;
        private readonly ILogger<DriverService> _logger;

        public DriverService(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            IValidator<CreateDriverRequest> createValidator,
            ILogger<DriverService> logger)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _createValidator = createValidator;
            _logger = logger;
        }

        public async Task<ApiResponse<DriverResponse>> CreateAsync(CreateDriverRequest request)
        {
            try
            {
                // Validar request
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<DriverResponse>.FailureResponse(
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // Verificar se CNPJ já existe
                if (await _unitOfWork.Drivers.ExistsByCNPJAsync(request.CNPJ))
                {
                    return ApiResponse<DriverResponse>.FailureResponse("CNPJ already exists");
                }

                // Verificar se CNH já existe
                if (await _unitOfWork.Drivers.ExistsByCNHAsync(request.CNHNumber))
                {
                    return ApiResponse<DriverResponse>.FailureResponse("CNH already exists");
                }

                // Criar entregador
                var driver = new Driver(
                    request.Identifier,
                    request.Name,
                    request.CNPJ,
                    request.BirthDate,
                    request.CNHNumber,
                    request.CNHType
                );

                await _unitOfWork.Drivers.AddAsync(driver);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Driver created: {DriverId}", driver.Id);

                return ApiResponse<DriverResponse>.SuccessResponse(
                    driver.ToResponse(),
                    "Driver created successfully"
                );
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return ApiResponse<DriverResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return ApiResponse<DriverResponse>.FailureResponse("An error occurred while creating the driver");
            }
        }

        public async Task<ApiResponse<DriverResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(id);

                if (driver == null)
                {
                    return ApiResponse<DriverResponse>.FailureResponse("Driver not found");
                }

                return ApiResponse<DriverResponse>.SuccessResponse(driver.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver by id: {DriverId}", id);
                return ApiResponse<DriverResponse>.FailureResponse("An error occurred while retrieving the driver");
            }
        }

        public async Task<ApiResponse<DriverResponse>> GetByIdentifierAsync(string identifier)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.GetByIdentifierAsync(identifier);

                if (driver == null)
                {
                    return ApiResponse<DriverResponse>.FailureResponse("Driver not found");
                }

                return ApiResponse<DriverResponse>.SuccessResponse(driver.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver by identifier: {Identifier}", identifier);
                return ApiResponse<DriverResponse>.FailureResponse("An error occurred while retrieving the driver");
            }
        }

        public async Task<ApiResponse<DriverResponse>> UpdateCNHImageAsync(Guid id, UpdateDriverCNHImageRequest request)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(id);

                if (driver == null)
                {
                    return ApiResponse<DriverResponse>.FailureResponse("Driver not found");
                }

                // Validar formato do arquivo
                if (!_storageService.IsValidImageFormat(request.FileName))
                {
                    return ApiResponse<DriverResponse>.FailureResponse("Invalid image format. Only PNG and BMP are allowed");
                }

                // Upload da imagem
                var imageUrl = await _storageService.UploadFileAsync(
                    request.ImageStream,
                    $"cnh/{driver.Id}/{request.FileName}",
                    request.ContentType
                );

                // Atualizar driver
                driver.UpdateCNHImage(imageUrl);

                await _unitOfWork.Drivers.UpdateAsync(driver);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Driver CNH image updated: {DriverId}", id);

                return ApiResponse<DriverResponse>.SuccessResponse(
                    driver.ToResponse(),
                    "CNH image updated successfully"
                );
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return ApiResponse<DriverResponse>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver CNH image");
                return ApiResponse<DriverResponse>.FailureResponse("An error occurred while updating the CNH image");
            }
        }
    }
}