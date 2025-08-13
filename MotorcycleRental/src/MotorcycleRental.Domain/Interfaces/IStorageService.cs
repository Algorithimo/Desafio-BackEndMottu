namespace MotorcycleRental.Domain.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<Stream?> DownloadFileAsync(string fileUrl);
        bool IsValidImageFormat(string fileName);
    }
}