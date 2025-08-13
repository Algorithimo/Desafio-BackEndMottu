using MotorcycleRental.Domain.Exceptions;

namespace MotorcycleRental.Domain.ValueObjects
{
    public record RentalPeriod
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; init; }
        public DateTime ExpectedEndDate { get; }

        public RentalPeriod(DateTime startDate, DateTime expectedEndDate)
        {
            if (startDate >= expectedEndDate)
                throw new BusinessRuleException("RENTAL_PERIOD_INVALID",
                    "Start date must be before expected end date");

            StartDate = startDate.Date;
            ExpectedEndDate = expectedEndDate.Date;
            EndDate = DateTime.MinValue; // Será definido na devolução
        }

        public int TotalDays => (ExpectedEndDate - StartDate).Days;

        public RentalPeriod WithReturnDate(DateTime returnDate)
        {
            return this with { EndDate = returnDate.Date };
        }
    }
}