using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.DTOs.Project;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectSummaryDto>> GetProjectsAsync(Guid userId);
        Task<ProjectDetailDto> GetProjectDetailAsync(Guid projectId, Guid userId);
        Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto dto, Guid userId);
        Task DeleteProjectAsync(Guid projectId, Guid userId);
        Task<Guid> SaveAnalysisAsProjectAsync(AnalysisResultDto result, string jobTitle, string jobDescription, string seniorityLevel, Stream cvStream, string originalFileName, Guid userId);
        Task<ProjectDetailDto> ConvertGuestAnalysisAsync(Guid analysisLogId, Guid userId);
    }
}