using MotorcycleRental.Domain.Exceptions;
using MotorcycleRental.Domain.ValueObjects;

namespace MotorcycleRental.Domain.Entities
{
    public class Motorcycle : BaseEntity
    {
        private readonly List<Rental> _rentals = new();

        public string Identifier { get; private set; }
        public int Year { get; private set; }
        public string Model { get; private set; }
        public Plate Plate { get; private set; }

        // Navigation
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        protected Motorcycle() { } // EF Core

        public Motorcycle(string identifier, int year, string model, string plate)
        {
            SetIdentifier(identifier);
            SetYear(year);
            SetModel(model);
            Plate = new Plate(plate);
        }

        public void UpdatePlate(string newPlate)
        {
            if (HasActiveRental())
                throw new InvalidDomainOperationException(
                    "Cannot update plate while motorcycle has active rentals");

            Plate = new Plate(newPlate);
            UpdateTimestamp();
        }

        public bool CanBeDeleted()
        {
            return !_rentals.Any();
        }

        public bool IsAvailable()
        {
            return !HasActiveRental();
        }

        private bool HasActiveRental()
        {
            return _rentals.Any(r => r.Status == Enums.RentalStatus.Active);
        }

        private void SetIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new BusinessRuleException("MOTORCYCLE_INVALID",
                    "Identifier cannot be empty");

            Identifier = identifier.Trim();
        }

        private void SetYear(int year)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (year < 1900 || year > currentYear + 1)
                throw new BusinessRuleException("MOTORCYCLE_INVALID",
                    $"Year must be between 1900 and {currentYear + 1}");

            Year = year;
        }

        private void SetModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new BusinessRuleException("MOTORCYCLE_INVALID",
                    "Model cannot be empty");

            Model = model.Trim();
        }
    }
}