using MotorcycleRental.Domain.Entities;
using MotorcycleRental.Domain.Enums;

namespace MotorcycleRental.Domain.Events
{
    public class MotorcycleEvent : BaseEntity
    {
        public Guid MotorcycleId { get; private set; }
        public EventType EventType { get; private set; }
        public int Year { get; private set; }
        public string EventData { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        protected MotorcycleEvent() { } // EF Core

        public MotorcycleEvent(
            Guid motorcycleId,
            EventType eventType,
            int year,
            string eventData)
        {
            MotorcycleId = motorcycleId;
            EventType = eventType;
            Year = year;
            EventData = eventData;
        }

        public void MarkAsProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }
}