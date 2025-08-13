namespace MotorcycleRental.Domain.Events
{
    public record MotorcycleCreatedEvent : IDomainEvent
    {
        public Guid Id { get; init; }
        public DateTime OccurredAt { get; init; }
        public Guid MotorcycleId { get; init; }
        public string Identifier { get; init; }
        public int Year { get; init; }
        public string Model { get; init; }
        public string Plate { get; init; }

        public MotorcycleCreatedEvent(
            Guid motorcycleId,
            string identifier,
            int year,
            string model,
            string plate)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            MotorcycleId = motorcycleId;
            Identifier = identifier;
            Year = year;
            Model = model;
            Plate = plate;
        }
    }
}