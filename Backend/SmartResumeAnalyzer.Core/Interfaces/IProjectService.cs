using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.DTOs.Project;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectSummaryDto>> GetProjectsAsync(Guid userId);
        Task<ProjectDetailDto> GetProjectDetailAsync(Guid projectId, Guid userId);
        Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto dto, Guid userId);
        Task<ProjectDetailDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid userId);
        Task<ProjectDetailDto> AddCvVersionAsync(Guid projectId, Stream cvStream, string originalFileName, string storedFileName, string notes, Guid userId);
        Task<CompareVersionsDto> CompareVersionsAsync(Guid projectId, Guid versionAId, Guid versionBId, Guid userId);
        Task DeleteProjectAsync(Guid projectId, Guid userId);
        Task<Guid> SaveAnalysisAsProjectAsync(AnalysisResultDto result, string jobTitle, string companyName, string jobDescription, string seniorityLevel, string industry, string storedFileName, string originalFileName, Guid userId, Guid? existingProjectId = null);
        Task<ProjectDetailDto> ConvertGuestAnalysisAsync(Guid analysisLogId, Guid userId);
    }
}