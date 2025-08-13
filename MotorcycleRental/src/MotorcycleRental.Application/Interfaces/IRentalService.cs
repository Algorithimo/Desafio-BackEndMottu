using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Application.DTOs.Rental;

namespace MotorcycleRental.Application.Interfaces
{
    public interface IRentalService
    {
        Task<ApiResponse<RentalResponse>> CreateAsync(CreateRentalRequest request);
        Task<ApiResponse<RentalResponse>> GetByIdAsync(Guid id);
        Task<ApiResponse<RentalReturnSimulationResponse>> SimulateReturnAsync(Guid id, SimulateReturnRequest request);
        Task<ApiResponse<RentalResponse>> ProcessReturnAsync(Guid id, ProcessReturnRequest request);
        Task<ApiResponse<IEnumerable<RentalResponse>>> GetByDriverIdAsync(Guid driverId);
    }
}