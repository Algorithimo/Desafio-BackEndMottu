using Microsoft.Extensions.Logging;
using MotorcycleRental.Domain.Events;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Infrastructure.Messaging
{
    /// <summary>
    /// Implementação temporária do publisher. 
    /// Será substituída pelo RabbitMQ na próxima fase.
    /// </summary>
    public class TempMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<TempMessagePublisher> _logger;

        public TempMessagePublisher(ILogger<TempMessagePublisher> logger)
        {
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, string? routingKey = null) where T : IDomainEvent
        {
            _logger.LogInformation(
                "Message published (TEMP): {MessageType} with Id {MessageId}",
                typeof(T).Name,
                message.Id
            );

            // Simular delay de publicação
            await Task.Delay(100);
        }
    }
}