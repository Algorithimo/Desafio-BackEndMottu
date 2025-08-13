using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Motorcycle;

namespace MotorcycleRental.Application.Interfaces
{
    public interface IMotorcycleService
    {
        Task<ApiResponse<MotorcycleResponse>> CreateAsync(CreateMotorcycleRequest request);
        Task<ApiResponse<MotorcycleResponse>> GetByIdAsync(Guid id);
        Task<ApiResponse<MotorcycleListResponse>> GetAllAsync(string? plateFilter = null);
        Task<ApiResponse<MotorcycleResponse>> UpdatePlateAsync(Guid id, UpdateMotorcyclePlateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
    }
}