using Microsoft.EntityFrameworkCore;
using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Data;

namespace MotorcycleRental.Infrastructure.Repositories
{
    public class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        public DriverRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Driver?> GetByCNPJAsync(string cnpj)
        {
            // Remove formatação
            var cleanCnpj = System.Text.RegularExpressions.Regex.Replace(cnpj, @"[^\d]", "");

            return await DbSet
                .FirstOrDefaultAsync(d => d.CNPJ.Value == cleanCnpj);
        }

        public async Task<Driver?> GetByCNHAsync(string cnhNumber)
        {
            // Remove formatação
            var cleanCnh = System.Text.RegularExpressions.Regex.Replace(cnhNumber, @"[^\d]", "");

            return await DbSet
                .FirstOrDefaultAsync(d => d.CNH.Number == cleanCnh);
        }

        public async Task<bool> ExistsByCNPJAsync(string cnpj)
        {
            var cleanCnpj = System.Text.RegularExpressions.Regex.Replace(cnpj, @"[^\d]", "");

            return await DbSet
                .AnyAsync(d => d.CNPJ.Value == cleanCnpj);
        }

        public async Task<bool> ExistsByCNHAsync(string cnhNumber)
        {
            var cleanCnh = System.Text.RegularExpressions.Regex.Replace(cnhNumber, @"[^\d]", "");

            return await DbSet
                .AnyAsync(d => d.CNH.Number == cleanCnh);
        }

        public async Task<Driver?> GetByIdentifierAsync(string identifier)
        {
            return await DbSet
                .Include(d => d.Rentals)
                .FirstOrDefaultAsync(d => d.Identifier == identifier);
        }

        public override async Task<Driver?> GetByIdAsync(Guid id)
        {
            return await DbSet
                .Include(d => d.Rentals)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}