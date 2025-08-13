using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Repositories;

namespace MotorcycleRental.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IMotorcycleRepository? _motorcycles;
        private IDriverRepository? _drivers;
        private IRentalRepository? _rentals;
        private IMotorcycleEventRepository? _motorcycleEvents;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IMotorcycleRepository Motorcycles =>
            _motorcycles ??= new MotorcycleRepository(_context);

        public IDriverRepository Drivers =>
            _drivers ??= new DriverRepository(_context);

        public IRentalRepository Rentals =>
            _rentals ??= new RentalRepository(_context);

        public IMotorcycleEventRepository MotorcycleEvents =>
            _motorcycleEvents ??= new MotorcycleEventRepository(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task RollbackAsync()
        {
            await Task.CompletedTask;
            // Com EF Core, simplesmente não chamar SaveChanges já é um rollback
            // Para cenários mais complexos, usar transações explícitas
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}