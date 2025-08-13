namespace MotorcycleRental.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IMotorcycleRepository Motorcycles { get; }
        IDriverRepository Drivers { get; }
        IRentalRepository Rentals { get; }
        IMotorcycleEventRepository MotorcycleEvents { get; }

        Task<int> CommitAsync();
        Task RollbackAsync();
    }
}