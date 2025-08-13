using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Application.DTOs.Driver
{
    public record DriverResponse(
        Guid Id,
        string Identifier,
        string Name,
        string CNPJ,
        DateTime BirthDate,
        string CNHNumber,
        CNHType CNHType,
        string? CNHImageUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}