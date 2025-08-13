using System.Text.RegularExpressions;
using MotorcycleRental.Domain.Exceptions;

namespace MotorcycleRental.Domain.ValueObjects
{
    public record CNH
    {
        public string Number { get; }

        public CNH(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new BusinessRuleException("CNH_INVALID", "CNH number cannot be empty");

            // Remove espaços e caracteres especiais
            number = Regex.Replace(number, @"[^\d]", "");

            if (number.Length != 11)
                throw new BusinessRuleException("CNH_INVALID", "CNH must have 11 digits");

            Number = number;
        }

        public override string ToString() => Number;
    }
}