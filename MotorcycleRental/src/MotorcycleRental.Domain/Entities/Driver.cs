using MotorcycleRental.Domain.Enums;
using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.ValueObjects;

namespace MotorcycleRental.Domain.Entities
{
    public class Driver : BaseEntity
    {
        private readonly List<Rental> _rentals = new();

        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public ValueObjects.CNPJ CNPJ { get; private set; }
        public DateTime BirthDate { get; private set; }
        public CNH CNH { get; private set; }
        public CNHType CNHType { get; private set; }
        public string? CNHImageUrl { get; private set; }

        // Navigation
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        protected Driver() { } // EF Core

        public Driver(
            string identifier,
            string name,
            string cnpj,
            DateTime birthDate,
            string cnhNumber,
            CNHType cnhType)
        {
            SetIdentifier(identifier);
            SetName(name);
            CNPJ = new ValueObjects.CNPJ(cnpj);
            SetBirthDate(birthDate);
            CNH = new CNH(cnhNumber);
            CNHType = cnhType;
        }

        public void UpdateCNHImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new BusinessRuleException("DRIVER_INVALID",
                    "CNH image URL cannot be empty");

            CNHImageUrl = imageUrl;
            UpdateTimestamp();
        }

        public bool CanRent()
        {
            // Deve ter CNH tipo A ou AB
            return CNHType == CNHType.A || CNHType == CNHType.AB;
        }

        public bool HasActiveRental()
        {
            return _rentals.Any(r => r.Status == RentalStatus.Active);
        }

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;

            if (BirthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

        private void SetIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new BusinessRuleException("DRIVER_INVALID",
                    "Identifier cannot be empty");

            Identifier = identifier.Trim();
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleException("DRIVER_INVALID",
                    "Name cannot be empty");

            if (name.Length < 3)
                throw new BusinessRuleException("DRIVER_INVALID",
                    "Name must have at least 3 characters");

            Name = name.Trim();
        }

        private void SetBirthDate(DateTime birthDate)
        {
            var minDate = DateTime.Today.AddYears(-100);
            var maxDate = DateTime.Today.AddYears(-18); // Mínimo 18 anos

            if (birthDate < minDate || birthDate > maxDate)
                throw new BusinessRuleException("DRIVER_INVALID",
                    "Driver must be at least 18 years old");

            BirthDate = birthDate.Date;
        }
    }
}