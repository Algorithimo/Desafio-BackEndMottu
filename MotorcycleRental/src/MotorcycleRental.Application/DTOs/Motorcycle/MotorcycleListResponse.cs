namespace MotorcycleRental.Application.DTOs.Motorcycle
{
    public record MotorcycleListResponse(
        IEnumerable<MotorcycleResponse> Motorcycles,
        int TotalCount
    );
}