using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotorcycleRental.Domain.Events;
using MotorcycleRental.Domain.Interfaces;
using RabbitMQ.Client;

namespace MotorcycleRental.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IMessagePublisher, IDisposable
    {
        private readonly RabbitMQOptions _options;
        private readonly ILogger<RabbitMQPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisher(
            IOptions<RabbitMQOptions> options,
            ILogger<RabbitMQPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange
                _channel.ExchangeDeclare(
                    exchange: _options.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );

                _logger.LogInformation("RabbitMQ Publisher connected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        public async Task PublishAsync<T>(T message, string? routingKey = null) where T : IDomainEvent
        {
            try
            {
                var messageBody = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = message.Id.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Type = typeof(T).Name;

                var actualRoutingKey = routingKey ?? _options.RoutingKey;

                _channel.BasicPublish(
                    exchange: _options.ExchangeName,
                    routingKey: actualRoutingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation(
                    "Message published: {MessageType} with Id {MessageId} to exchange {Exchange} with routing key {RoutingKey}",
                    typeof(T).Name,
                    message.Id,
                    _options.ExchangeName,
                    actualRoutingKey
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ Publisher disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
        }
    }
}