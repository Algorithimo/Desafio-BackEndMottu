using MotorcycleRental.Domain.Events;

namespace MotorcycleRental.Domain.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string? routingKey = null) where T : IDomainEvent;
    }
}
