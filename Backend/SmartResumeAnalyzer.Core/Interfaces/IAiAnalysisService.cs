using SmartResumeAnalyzer.Core.DTOs.Analysis;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAiAnalysisService
    {
        Task<AnalysisResultDto> AnalyzeAsync(string cvText, string jobTitle, string companyName, string jobDescription, string seniorityLevel);
    }
}