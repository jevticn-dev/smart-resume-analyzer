using SmartResumeAnalyzer.Core.Entities;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetProjectsByUserIdAsync(Guid userId);
        Task<Project?> GetProjectWithDetailsAsync(Guid projectId, Guid userId);
        Task<Project?> GetProjectWithAllVersionsAsync(Guid projectId, Guid userId);
        Task<Project?> GetByIdForUserAsync(Guid projectId, Guid userId);
        Task AddAsync(Project project);
        Task DeleteAsync(Project project);
        Task SaveChangesAsync();
    }
}