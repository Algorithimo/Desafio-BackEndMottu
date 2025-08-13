using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Domain.Interfaces
{
    public interface IDriverRepository : IBaseRepository<Driver>
    {
        Task<Driver?> GetByCNPJAsync(string cnpj);
        Task<Driver?> GetByCNHAsync(string cnhNumber);
        Task<bool> ExistsByCNPJAsync(string cnpj);
        Task<bool> ExistsByCNHAsync(string cnhNumber);
        Task<Driver?> GetByIdentifierAsync(string identifier);
    }
}