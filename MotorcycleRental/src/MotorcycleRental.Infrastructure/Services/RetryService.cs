using Microsoft.Extensions.Logging;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Infrastructure.Services
{
    public class RetryService : IRetryService 
    {
        private readonly ILogger<RetryService> _logger;

        public RetryService(ILogger<RetryService> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await operation();
                    return; //Sucesso
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("🔄 Retry {Attempt}/{MaxRetries} - Erro: {Error}",
                        attempt, maxRetries, ex.Message);

                    if (attempt == maxRetries)
                    {
                        throw; //Última tentativa falhou
                    }

                    await Task.Delay(1000); // ⏱️ Espera 1s
                }
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation(); //Sucesso
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("🔄 Retry {Attempt}/{MaxRetries} - Erro: {Error}",
                        attempt, maxRetries, ex.Message);

                    if (attempt == maxRetries)
                    {
                        throw; // Última tentativa falhou
                    }

                    await Task.Delay(1000); // Espera 1s
                }
            }

            throw new InvalidOperationException("Unexpected end of retry loop");
        }
    }
}