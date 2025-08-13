using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MotorcycleRental.Application.Interfaces;
using MotorcycleRental.Application.Services;
using MotorcycleRental.Application.Validators;

namespace MotorcycleRental.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registrar Services
            services.AddScoped<IMotorcycleService, MotorcycleService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IRentalService, RentalService>();

            // Registrar Validators - usando uma classe concreta do assembly
            services.AddValidatorsFromAssemblyContaining<CreateMotorcycleRequestValidator>();

            return services;
        }
    }
}