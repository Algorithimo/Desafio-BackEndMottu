using Microsoft.EntityFrameworkCore;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Data;

namespace MotorcycleRental.Infrastructure.Repositories
{
    public class MotorcycleRepository : BaseRepository<Motorcycle>, IMotorcycleRepository
    {
        public MotorcycleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Motorcycle?> GetByPlateAsync(string plate)
        {
            return await DbSet
                .FirstOrDefaultAsync(m => m.Plate.Value == plate.ToUpper());
        }

        public async Task<bool> ExistsByPlateAsync(string plate)
        {
            return await DbSet
                .AnyAsync(m => m.Plate.Value == plate.ToUpper());
        }

        public async Task<IEnumerable<Motorcycle>> GetByPlateFilterAsync(string? plateFilter)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(plateFilter))
            {
                var upperFilter = plateFilter.ToUpper();
                query = query.Where(m => m.Plate.Value.Contains(upperFilter));
            }

            return await query.OrderBy(m => m.Plate.Value).ToListAsync();
        }

        public async Task<bool> HasRentalsAsync(Guid motorcycleId)
        {
            return await Context.Rentals
                .AnyAsync(r => r.MotorcycleId == motorcycleId);
        }

        public async Task<IEnumerable<Motorcycle>> GetAvailableMotorcyclesAsync()
        {
            var rentedMotorcycleIds = await Context.Rentals
                .Where(r => r.Status == Domain.Enums.RentalStatus.Active)
                .Select(r => r.MotorcycleId)
                .ToListAsync();

            return await DbSet
                .Where(m => !rentedMotorcycleIds.Contains(m.Id))
                .OrderBy(m => m.Model)
                .ToListAsync();
        }

        public override async Task<Motorcycle?> GetByIdAsync(Guid id)
        {
            return await DbSet
                .Include(m => m.Rentals)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}