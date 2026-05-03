namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream fileStream, string fileName);
        Task<byte[]> ReadAsync(string storedFileName);
        void Delete(string storedFileName);
    }
}