using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.Interfaces;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public FileStorageService(IOptions<FileStorageSettings> settings)
        {
            _basePath = settings.Value.BasePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(Stream fileStream, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_basePath, storedFileName);

            using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);

            return storedFileName;
        }
        public async Task<byte[]> ReadAsync(string storedFileName)
        {
            var path = Path.Combine(_basePath, storedFileName);
            return await File.ReadAllBytesAsync(path);
        }

        public void Delete(string storedFileName)
        {
            var filePath = Path.Combine(_basePath, storedFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}