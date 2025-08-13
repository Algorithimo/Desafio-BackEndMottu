using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotorcycleRental.Domain.Enums;
using MotorcycleRental.Domain.Events;
using MotorcycleRental.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MotorcycleRental.Infrastructure.Messaging
{
    public class MotorcycleEventConsumer : BackgroundService
    {
        private readonly RabbitMQOptions _options;
        private readonly ILogger<MotorcycleEventConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;

        // CONFIGURAÇÕES DE RETRY
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int BASE_DELAY_MS = 1000;

        public MotorcycleEventConsumer(
            IOptions<RabbitMQOptions> options,
            ILogger<MotorcycleEventConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            try
            {
                InitializeRabbitMQ();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    await ProcessMessageWithRetry(ea);
                };

                _channel.BasicConsume(
                    queue: _options.QueueName,
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("Motorcycle Event Consumer started");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in MotorcycleEventConsumer");
                throw;
            }
        }

        //MÉTODO COM RETRY E PROTEÇÃO
        private async Task ProcessMessageWithRetry(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var deliveryTag = ea.DeliveryTag;

            _logger.LogInformation("Processing message: {MessageId}", ea.BasicProperties?.MessageId ?? "unknown");

            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    //TENTAR PROCESSAR
                    await ProcessMessage(message);

                    //SUCESSO - ACK da mensagem
                    _channel?.BasicAck(deliveryTag, multiple: false);
                    _logger.LogInformation("Message processed successfully on attempt {Attempt}", attempt);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message on attempt {Attempt}/{MaxAttempts}",
                        attempt, MAX_RETRY_ATTEMPTS);

                    if (attempt == MAX_RETRY_ATTEMPTS)
                    {
                        //FALHOU TODAS AS TENTATIVAS
                        await HandleFailedMessage(ea, ex);
                        return;
                    }

                    // EXPONENTIAL BACKOFF
                    var delay = TimeSpan.FromMilliseconds(BASE_DELAY_MS * Math.Pow(2, attempt - 1));
                    _logger.LogWarning("Retrying in {Delay}ms...", delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }
            }
        }

        //LIDAR COM MENSAGENS QUE FALHARAM TODAS AS TENTATIVAS
        private async Task HandleFailedMessage(BasicDeliverEventArgs ea, Exception lastException)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogError(lastException,
                "Message failed after {MaxAttempts} attempts. Sending to DLQ: {Message}",
                MAX_RETRY_ATTEMPTS, message);

            try
            {
                //ENVIAR PARA DEAD LETTER QUEUE
                await SendToDeadLetterQueue(message, lastException.Message);

                //ACK a mensagem original (para remover da fila principal)
                _channel?.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to DLQ. NACK and requeue.");

                //ÚLTIMO RECURSO: NACK sem requeue (descarta mensagem)
                _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }

        // DEAD LETTER QUEUE
        private Task SendToDeadLetterQueue(string originalMessage, string errorMessage)
        {
            try
            {
                var dlqMessage = new
                {
                    OriginalMessage = originalMessage,
                    ErrorMessage = errorMessage,
                    FailedAt = DateTime.UtcNow,
                    Attempts = MAX_RETRY_ATTEMPTS
                };

                var dlqBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dlqMessage));
                var dlqProperties = _channel!.CreateBasicProperties();
                dlqProperties.Persistent = true;

                // Publicar na DLQ (você precisa criar essa fila)
                _channel.BasicPublish(
                    exchange: _options.ExchangeName,
                    routingKey: "motorcycle.failed", // Routing key para DLQ
                    basicProperties: dlqProperties,
                    body: dlqBody
                );
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Dead Letter Queue");
                throw;
            }
        }

        // PROCESSAR MENSAGEM
        private async Task ProcessMessage(string message)
        {
            var motorcycleCreatedEvent = JsonSerializer.Deserialize<MotorcycleCreatedEvent>(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (motorcycleCreatedEvent != null && motorcycleCreatedEvent.Year == 2024)
            {
                await SaveMotorcycleEvent(motorcycleCreatedEvent);
                _logger.LogInformation("Motorcycle event for year 2024 processed and saved");
            }
            else
            {
                _logger.LogInformation("Message processed but not saved (year != 2024)");
            }
        }

        private async Task SaveMotorcycleEvent(MotorcycleCreatedEvent createdEvent)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var motorcycleEvent = new MotorcycleEvent(
                createdEvent.MotorcycleId,
                EventType.MotorcycleCreated,
                createdEvent.Year,
                JsonSerializer.Serialize(createdEvent)
            );

            var eventRepository = scope.ServiceProvider.GetRequiredService<IMotorcycleEventRepository>();
            await eventRepository.AddAsync(motorcycleEvent);
            await unitOfWork.CommitAsync();

            motorcycleEvent.MarkAsProcessed();
            await eventRepository.UpdateAsync(motorcycleEvent);
            await unitOfWork.CommitAsync();
        }

        private void InitializeRabbitMQ()
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

            // Declarar fila principal
            _channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // DECLARAR DEAD LETTER QUEUE
            _channel.QueueDeclare(
                queue: "motorcycle.failed.dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // Bind da fila principal
            _channel.QueueBind(
                queue: _options.QueueName,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey
            );

            // BIND DA DLQ
            _channel.QueueBind(
                queue: "motorcycle.failed.dlq",
                exchange: _options.ExchangeName,
                routingKey: "motorcycle.failed"
            );

            _logger.LogInformation("RabbitMQ Consumer initialized with DLQ support");
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            base.Dispose();
            _logger.LogInformation("MotorcycleEventConsumer disposed");
        }
    }
}