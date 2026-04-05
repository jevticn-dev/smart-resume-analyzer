namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream fileStream, string fileName);
        void Delete(string storedFileName);
    }
}