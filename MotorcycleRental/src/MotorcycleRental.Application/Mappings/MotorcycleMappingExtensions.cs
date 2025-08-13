using MotorcycleRental.Application.DTOs.Motorcycle;
using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Application.Mappings
{
    public static class MotorcycleMappingExtensions
    {
        public static MotorcycleResponse ToResponse(this Motorcycle motorcycle)
        {
            return new MotorcycleResponse(
                motorcycle.Id,
                motorcycle.Identifier,
                motorcycle.Year,
                motorcycle.Model,
                motorcycle.Plate.Value,
                motorcycle.CreatedAt,
                motorcycle.UpdatedAt
            );
        }

        public static IEnumerable<MotorcycleResponse> ToResponse(this IEnumerable<Motorcycle> motorcycles)
        {
            return motorcycles.Select(m => m.ToResponse());
        }
    }
}