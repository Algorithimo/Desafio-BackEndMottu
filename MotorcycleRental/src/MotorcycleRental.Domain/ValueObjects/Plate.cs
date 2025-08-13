using System.Text.RegularExpressions;
using MotorcycleRental.Domain.Exceptions;

namespace MotorcycleRental.Domain.ValueObjects
{
    public record Plate
    {
        public string Value { get; }

        public Plate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleException("PLATE_INVALID", "Plate cannot be empty");

            value = value.ToUpper().Trim();

            // Validar formato brasileiro: ABC1234 ou ABC1D23 (Mercosul)
            if (!Regex.IsMatch(value, @"^[A-Z]{3}[0-9]{1}[A-Z0-9]{1}[0-9]{2}$"))
                throw new BusinessRuleException("PLATE_INVALID", "Plate format is invalid");

            Value = value;
        }

        public override string ToString() => Value;
    }
}