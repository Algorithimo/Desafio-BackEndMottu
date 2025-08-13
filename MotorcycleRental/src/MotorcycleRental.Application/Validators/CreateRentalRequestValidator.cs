using FluentValidation;
using MotorcycleRental.Application.DTOs.Rental;

namespace MotorcycleRental.Application.Validators
{
    public class CreateRentalRequestValidator : AbstractValidator<CreateRentalRequest>
    {
        public CreateRentalRequestValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("Driver ID is required");

            RuleFor(x => x.MotorcycleId)
                .NotEmpty().WithMessage("Motorcycle ID is required");

            RuleFor(x => x.Plan)
                .IsInEnum().WithMessage("Invalid rental plan");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .Must(BeAFutureDate).WithMessage("Start date must be tomorrow or later");
        }

        private bool BeAFutureDate(DateTime startDate)
        {
            // Start date must be at least tomorrow
            return startDate.Date > DateTime.Today;
        }
    }
}