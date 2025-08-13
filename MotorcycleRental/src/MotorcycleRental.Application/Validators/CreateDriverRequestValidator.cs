using FluentValidation;
using MotorcycleRental.Application.DTOs.Driver;
using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Application.Validators
{
    public class CreateDriverRequestValidator : AbstractValidator<CreateDriverRequest>
    {
        public CreateDriverRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier is required")
                .MaximumLength(50).WithMessage("Identifier must not exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must have at least 3 characters")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

            RuleFor(x => x.CNPJ)
                .NotEmpty().WithMessage("CNPJ is required")
                .Must(BeValidCNPJFormat).WithMessage("CNPJ must have 14 digits");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .Must(BeAtLeast18YearsOld).WithMessage("Driver must be at least 18 years old");

            RuleFor(x => x.CNHNumber)
                .NotEmpty().WithMessage("CNH number is required")
                .Must(BeValidCNHFormat).WithMessage("CNH must have 11 digits");

            RuleFor(x => x.CNHType)
                .IsInEnum().WithMessage("Invalid CNH type");
        }

        private bool BeValidCNPJFormat(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            var cleanCnpj = System.Text.RegularExpressions.Regex.Replace(cnpj, @"[^\d]", "");
            return cleanCnpj.Length == 14;
        }

        private bool BeValidCNHFormat(string cnh)
        {
            if (string.IsNullOrWhiteSpace(cnh))
                return false;

            var cleanCnh = System.Text.RegularExpressions.Regex.Replace(cnh, @"[^\d]", "");
            return cleanCnh.Length == 11;
        }

        private bool BeAtLeast18YearsOld(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}