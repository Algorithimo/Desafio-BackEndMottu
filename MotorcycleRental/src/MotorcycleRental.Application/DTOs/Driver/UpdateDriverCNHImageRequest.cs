namespace MotorcycleRental.Application.DTOs.Driver
{
    public record UpdateDriverCNHImageRequest
    {
        public Stream ImageStream { get; init; } = null!;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
    }
}