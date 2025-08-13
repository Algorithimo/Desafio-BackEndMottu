using MotorcycleRental.Application.DTOs.Rental;
using MotorcycleRental.Domain.Entities;

namespace MotorcycleRental.Application.Mappings
{
    public static class RentalMappingExtensions
    {
        public static RentalResponse ToResponse(this Rental rental)
        {
            return new RentalResponse(
                rental.Id,
                rental.MotorcycleId,
                rental.DriverId,
                rental.Plan,
                rental.Period.StartDate,
                rental.Period.ExpectedEndDate,
                rental.Period.EndDate == DateTime.MinValue ? null : rental.Period.EndDate,
                rental.DailyRate,
                rental.TotalAmount,
                rental.Status,
                rental.ReturnDate,
                rental.PenaltyAmount,
                rental.AdditionalAmount,
                rental.FinalAmount,
                rental.CreatedAt,
                rental.UpdatedAt
            );
        }

        public static RentalReturnSimulationResponse ToSimulationResponse(
            this RentalReturnResult result,
            decimal dailyRate,
            string? message = null)
        {
            return new RentalReturnSimulationResponse(
                result.ReturnDate,
                result.ActualDays,
                dailyRate,
                result.ActualDays * dailyRate,
                result.PenaltyAmount,
                result.AdditionalAmount,
                result.FinalAmount,
                message
            );
        }
    }
}