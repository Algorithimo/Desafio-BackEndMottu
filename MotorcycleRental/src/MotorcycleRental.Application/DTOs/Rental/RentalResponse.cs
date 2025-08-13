using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Application.DTOs.Rental
{
    public record RentalResponse(
        Guid Id,
        Guid MotorcycleId,
        Guid DriverId,
        RentalPlan Plan,
        DateTime StartDate,
        DateTime ExpectedEndDate,
        DateTime? EndDate,
        decimal DailyRate,
        decimal TotalAmount,
        RentalStatus Status,
        DateTime? ReturnDate,
        decimal? PenaltyAmount,
        decimal? AdditionalAmount,
        decimal? FinalAmount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}