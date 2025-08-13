using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Application.DTOs.Rental
{
    public record CreateRentalRequest(
        Guid DriverId,
        Guid MotorcycleId,
        RentalPlan Plan,
        DateTime StartDate
    );
}
