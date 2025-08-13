﻿namespace MotorcycleRental.Infrastructure.Messaging
{
    public class RabbitMQOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "motorcycle.events";
        public string QueueName { get; set; } = "motorcycle.created.2024";
        public string RoutingKey { get; set; } = "motorcycle.created";
    }
}