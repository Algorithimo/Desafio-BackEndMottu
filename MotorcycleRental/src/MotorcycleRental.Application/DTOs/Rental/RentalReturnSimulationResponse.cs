namespace MotorcycleRental.Application.DTOs.Rental
{
    public record RentalReturnSimulationResponse(
        DateTime ReturnDate,
        int TotalDays,
        decimal DailyRate,
        decimal BaseAmount,
        decimal PenaltyAmount,
        decimal AdditionalAmount,
        decimal FinalAmount,
        string? Message
    );
}