using MotorcycleRental.Domain.Events;

namespace MotorcycleRental.Domain.Interfaces
{
    public interface IMotorcycleEventRepository : IBaseRepository<MotorcycleEvent>
    {
        Task<IEnumerable<MotorcycleEvent>> GetByYearAsync(int year);
        Task<IEnumerable<MotorcycleEvent>> GetUnprocessedEventsAsync();
    }
}