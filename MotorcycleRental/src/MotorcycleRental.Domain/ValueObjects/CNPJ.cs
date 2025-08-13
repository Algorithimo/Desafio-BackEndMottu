using System.Text.RegularExpressions;
using MotorcycleRental.Domain.Exceptions;

namespace MotorcycleRental.Domain.ValueObjects
{
    public record CNPJ 
    {
        public string Value { get; }

        public CNPJ(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleException("CNPJ_INVALID", "CNPJ cannot be empty");

            // Remove formatação
            value = Regex.Replace(value, @"[^\d]", "");

            if (value.Length != 14)
                throw new BusinessRuleException("CNPJ_INVALID", "CNPJ must have 14 digits");

            if (!IsValid(value))
                throw new BusinessRuleException("CNPJ_INVALID", "CNPJ is not valid");

            Value = value;
        }

        private static bool IsValid(string cnpj)
        {
            // Validação simplificada - em produção, usar algoritmo completo
            // Verifica se todos os dígitos são iguais
            if (cnpj.Distinct().Count() == 1)
                return false;

            return true;
        }

        public string Formatted()
        {
            return $"{Value.Substring(0, 2)}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}";
        }

        public override string ToString() => Value;
    }
}