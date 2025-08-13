using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Domain.Interfaces
{
    public interface IRentalRepository : IBaseRepository<Rental>
    {
        Task<IEnumerable<Rental>> GetByDriverIdAsync(Guid driverId);
        Task<IEnumerable<Rental>> GetByMotorcycleIdAsync(Guid motorcycleId);
        Task<Rental?> GetActiveRentalByDriverIdAsync(Guid driverId);
        Task<Rental?> GetActiveRentalByMotorcycleIdAsync(Guid motorcycleId);
        Task<IEnumerable<Rental>> GetByStatusAsync(RentalStatus status);
        Task<bool> HasActiveRentalAsync(Guid motorcycleId);
    }
}