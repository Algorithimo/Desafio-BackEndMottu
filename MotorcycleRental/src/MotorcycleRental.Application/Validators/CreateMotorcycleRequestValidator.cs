using FluentValidation;
using MotorcycleRental.Application.DTOs.Motorcycle;

namespace MotorcycleRental.Application.Validators
{
    public class CreateMotorcycleRequestValidator : AbstractValidator<CreateMotorcycleRequest>
    {
        public CreateMotorcycleRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier is required")
                .MaximumLength(50).WithMessage("Identifier must not exceed 50 characters");

            RuleFor(x => x.Year)
                .InclusiveBetween(1900, DateTime.Now.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.Now.Year + 1}");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required")
                .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

            RuleFor(x => x.Plate)
                .NotEmpty().WithMessage("Plate is required")
                .Matches(@"^[A-Z]{3}[0-9]{1}[A-Z0-9]{1}[0-9]{2}$")
                .WithMessage("Plate must be in format ABC1234 or ABC1D23");
        }
    }
}