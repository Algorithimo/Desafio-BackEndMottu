using Microsoft.Extensions.Logging;
using MotorcycleRental.Domain.Interfaces;

namespace MotorcycleRental.Infrastructure.Storage
{
    /// <summary>
    /// Implementação temporária do storage. 
    /// Será substituída por storage real (local/S3/MinIO) na próxima fase.
    /// </summary>
    public class TempStorageService : IStorageService
    {
        private readonly ILogger<TempStorageService> _logger;
        private readonly string _basePath;

        public TempStorageService(ILogger<TempStorageService> logger)
        {
            _logger = logger;
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "temp-storage");

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var filePath = Path.Combine(_basePath, fileName);
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileOutputStream = File.Create(filePath))
            {
                await fileStream.CopyToAsync(fileOutputStream);
            }

            _logger.LogInformation("File uploaded (TEMP): {FileName}", fileName);

            // Retornar URL fictícia
            return $"/temp-storage/{fileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            _logger.LogInformation("File deleted (TEMP): {FileUrl}", fileUrl);
            return Task.FromResult(true);
        }

        public Task<Stream?> DownloadFileAsync(string fileUrl)
        {
            var memoryStream = new MemoryStream();
            return Task.FromResult<Stream?>(memoryStream);
        }

        public bool IsValidImageFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            return extension == ".png" || extension == ".bmp";
        }
    }
}