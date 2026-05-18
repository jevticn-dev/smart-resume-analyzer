using SmartResumeAnalyzer.Core.Entities;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAnalysisLogService
    {
        Task<AnalysisLog> CreateLogAsync(
            Guid? userId,
            string? ipAddress,
            string jobTitle,
            string companyName,
            string jobDescription,
            string seniorityLevel,
            string originalFileName,
            string storedFileName,
            string resultJson);
    }
}