namespace MotorcycleRental.Application.DTOs.Motorcycle
{
    public record MotorcycleResponse(
        Guid Id,
        string Identifier,
        int Year,
        string Model,
        string Plate,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}