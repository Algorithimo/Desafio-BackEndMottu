using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Driver;

namespace MotorcycleRental.Application.Interfaces
{
    public interface IDriverService
    {
        Task<ApiResponse<DriverResponse>> CreateAsync(CreateDriverRequest request);
        Task<ApiResponse<DriverResponse>> GetByIdAsync(Guid id);
        Task<ApiResponse<DriverResponse>> GetByIdentifierAsync(string identifier);
        Task<ApiResponse<DriverResponse>> UpdateCNHImageAsync(Guid id, UpdateDriverCNHImageRequest request);
    }
}