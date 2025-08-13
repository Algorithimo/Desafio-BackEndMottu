using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Domain.Interfaces
{
    public interface IMotorcycleRepository : IBaseRepository<Motorcycle>
    {
        Task<Motorcycle?> GetByPlateAsync(string plate);
        Task<bool> ExistsByPlateAsync(string plate);
        Task<IEnumerable<Motorcycle>> GetByPlateFilterAsync(string? plateFilter);
        Task<bool> HasRentalsAsync(Guid motorcycleId);
        Task<IEnumerable<Motorcycle>> GetAvailableMotorcyclesAsync();
    }
}