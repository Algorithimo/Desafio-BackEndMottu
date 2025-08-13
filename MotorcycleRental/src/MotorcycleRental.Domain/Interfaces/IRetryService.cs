namespace MotorcycleRental.Domain.Interfaces
{
    public interface IRetryService
    {
        Task ExecuteAsync(Func<Task> operation);
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    }
}