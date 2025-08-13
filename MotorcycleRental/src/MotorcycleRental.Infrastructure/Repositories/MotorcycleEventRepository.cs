using Microsoft.EntityFrameworkCore;
using MotorcycleRental.Domain.Events;
using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Data;

namespace MotorcycleRental.Infrastructure.Repositories
{
    public class MotorcycleEventRepository : BaseRepository<MotorcycleEvent>, IMotorcycleEventRepository
    {
        public MotorcycleEventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MotorcycleEvent>> GetByYearAsync(int year)
        {
            return await DbSet
                .Where(e => e.Year == year)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MotorcycleEvent>> GetUnprocessedEventsAsync()
        {
            return await DbSet
                .Where(e => e.ProcessedAt == null)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}