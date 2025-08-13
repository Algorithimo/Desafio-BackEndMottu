using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotorcycleRental.Domain.Interfaces;
using MotorcycleRental.Infrastructure.Data;
using MotorcycleRental.Infrastructure.Messaging;
using MotorcycleRental.Infrastructure.Repositories;
using MotorcycleRental.Infrastructure.Services;
using MotorcycleRental.Infrastructure.Storage;

namespace MotorcycleRental.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                )
            );

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IRentalRepository, RentalRepository>();
            services.AddScoped<IMotorcycleEventRepository, MotorcycleEventRepository>();

            // RabbitMQ Configuration
            services.Configure<RabbitMQOptions>(
                configuration.GetSection("RabbitMQ"));

            // Messaging - Usar RabbitMQ se configurado, senão usar temporário
            var rabbitMQSection = configuration.GetSection("RabbitMQ");
            var rabbitMQHostName = rabbitMQSection["HostName"];

            if (!string.IsNullOrEmpty(rabbitMQHostName))
            {
                services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
                services.AddHostedService<MotorcycleEventConsumer>();
            }
            else
            {
                services.AddSingleton<IMessagePublisher, TempMessagePublisher>();
            }

            // Storage (temporário)
            services.AddScoped<IStorageService, TempStorageService>();
            
            // Retry Pollicy
            services.AddScoped<IRetryService, RetryService>();

            return services;
        }
    }
}