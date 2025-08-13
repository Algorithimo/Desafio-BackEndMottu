using MotorcycleRental.Domain.Enums;
using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.ValueObjects;

namespace MotorcycleRental.Domain.Entities
{
    public class Rental : BaseEntity
    {
        public Guid MotorcycleId { get; private set; }
        public Guid DriverId { get; private set; }
        public RentalPlan Plan { get; private set; }
        public RentalPeriod Period { get; private set; }
        public decimal DailyRate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public RentalStatus Status { get; private set; }

        // Valores de devolução
        public DateTime? ReturnDate { get; private set; }
        public decimal? PenaltyAmount { get; private set; }
        public decimal? AdditionalAmount { get; private set; }
        public decimal? FinalAmount { get; private set; }

        // Navigation
        public virtual Motorcycle Motorcycle { get; private set; } = null!;
        public virtual Driver Driver { get; private set; } = null!;

        protected Rental() { } // EF Core

        public Rental(
            Guid motorcycleId,
            Guid driverId,
            RentalPlan plan,
            DateTime startDate)
        {
            MotorcycleId = motorcycleId;
            DriverId = driverId;
            Plan = plan;
            DailyRate = GetDailyRate(plan);

            var days = (int)plan;
            var expectedEndDate = startDate.AddDays(days);
            Period = new RentalPeriod(startDate, expectedEndDate);

            TotalAmount = DailyRate * days;
            Status = RentalStatus.Active;
        }

        public RentalReturnResult SimulateReturn(DateTime returnDate)
        {
            if (Status != RentalStatus.Active)
                throw new InvalidDomainOperationException("Rental is not active");

            var result = CalculateReturnValues(returnDate);
            return result;
        }

        public void ProcessReturn(DateTime returnDate)
        {
            if (Status != RentalStatus.Active)
                throw new InvalidDomainOperationException("Rental is not active");

            var result = CalculateReturnValues(returnDate);

            ReturnDate = returnDate;
            PenaltyAmount = result.PenaltyAmount;
            AdditionalAmount = result.AdditionalAmount;
            FinalAmount = result.FinalAmount;
            Status = RentalStatus.Completed;

            UpdateTimestamp();
        }

        private RentalReturnResult CalculateReturnValues(DateTime returnDate)
        {
            var expectedDays = (int)Plan;
            var actualDays = (returnDate.Date - Period.StartDate).Days;

            decimal penalty = 0;
            decimal additional = 0;
            decimal final = TotalAmount;

            if (actualDays < expectedDays)
            {
                // Devolução antecipada - calcular multa
                var unusedDays = expectedDays - actualDays;
                var unusedAmount = unusedDays * DailyRate;
                var penaltyRate = GetPenaltyRate(Plan);

                penalty = unusedAmount * penaltyRate;
                final = (actualDays * DailyRate) + penalty;
            }
            else if (actualDays > expectedDays)
            {
                // Devolução atrasada - calcular adicional
                var extraDays = actualDays - expectedDays;
                additional = extraDays * 50m; // R$ 50 por dia adicional
                final = TotalAmount + additional;
            }

            return new RentalReturnResult(
                returnDate,
                actualDays,
                penalty,
                additional,
                final
            );
        }

        private static decimal GetDailyRate(RentalPlan plan)
        {
            return plan switch
            {
                RentalPlan.Days7 => 30m,
                RentalPlan.Days15 => 28m,
                RentalPlan.Days30 => 22m,
                RentalPlan.Days45 => 20m,
                RentalPlan.Days50 => 18m,
                _ => throw new BusinessRuleException("RENTAL_INVALID", "Invalid rental plan")
            };
        }

        private static decimal GetPenaltyRate(RentalPlan plan)
        {
            return plan switch
            {
                RentalPlan.Days7 => 0.20m,  // 20%
                RentalPlan.Days15 => 0.40m, // 40%
                _ => 0m // Outros planos não têm multa especificada
            };
        }
    }

    public record RentalReturnResult(
        DateTime ReturnDate,
        int ActualDays,
        decimal PenaltyAmount,
        decimal AdditionalAmount,
        decimal FinalAmount
    );
}