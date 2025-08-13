namespace MotorcycleRental.Domain.Exceptions
{
    public class InvalidDomainOperationException : DomainException
    {
        public InvalidDomainOperationException(string message) : base(message)
        {
        }
    }
}