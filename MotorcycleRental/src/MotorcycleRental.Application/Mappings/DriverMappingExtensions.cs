using MotorcycleRental.Application.DTOs.Driver;
using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Application.Mappings
{
    public static class DriverMappingExtensions
    {
        public static DriverResponse ToResponse(this Driver driver)
        {
            return new DriverResponse(
                driver.Id,
                driver.Identifier,
                driver.Name,
                driver.CNPJ.Value,
                driver.BirthDate,
                driver.CNH.Number,
                driver.CNHType,
                driver.CNHImageUrl,
                driver.CreatedAt,
                driver.UpdatedAt
            );
        }
    }
}