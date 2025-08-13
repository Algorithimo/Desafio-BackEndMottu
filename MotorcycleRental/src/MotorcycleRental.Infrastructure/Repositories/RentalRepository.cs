using Microsoft.EntityFrameworkCore;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Enums;
using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Data;

namespace MotorcycleRental.Infrastructure.Repositories
{
    public class RentalRepository : BaseRepository<Rental>, IRentalRepository
    {
        public RentalRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Rental>> GetByDriverIdAsync(Guid driverId)
        {
            return await DbSet
                .Include(r => r.Motorcycle)
                .Where(r => r.DriverId == driverId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetByMotorcycleIdAsync(Guid motorcycleId)
        {
            return await DbSet
                .Include(r => r.Driver)
                .Where(r => r.MotorcycleId == motorcycleId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Rental?> GetActiveRentalByDriverIdAsync(Guid driverId)
        {
            return await DbSet
                .Include(r => r.Motorcycle)
                .FirstOrDefaultAsync(r => r.DriverId == driverId && r.Status == RentalStatus.Active);
        }

        public async Task<Rental?> GetActiveRentalByMotorcycleIdAsync(Guid motorcycleId)
        {
            return await DbSet
                .Include(r => r.Driver)
                .FirstOrDefaultAsync(r => r.MotorcycleId == motorcycleId && r.Status == RentalStatus.Active);
        }

        public async Task<IEnumerable<Rental>> GetByStatusAsync(RentalStatus status)
        {
            return await DbSet
                .Include(r => r.Motorcycle)
                .Include(r => r.Driver)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasActiveRentalAsync(Guid motorcycleId)
        {
            return await DbSet
                .AnyAsync(r => r.MotorcycleId == motorcycleId && r.Status == RentalStatus.Active);
        }

        public override async Task<Rental?> GetByIdAsync(Guid id)
        {
            return await DbSet
                .Include(r => r.Motorcycle)
                .Include(r => r.Driver)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}