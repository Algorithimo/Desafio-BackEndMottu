namespace MotorcycleRental.Application.DTOs.Motorcycle
{
    public record CreateMotorcycleRequest(
        string Identifier,
        int Year,
        string Model,
        string Plate
    );
}