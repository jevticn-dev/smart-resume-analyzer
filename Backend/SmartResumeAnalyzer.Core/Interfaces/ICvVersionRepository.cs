using SmartResumeAnalyzer.Core.Entities;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface ICvVersionRepository
    {
        Task<CvVersion?> GetByIdWithProjectAsync(Guid versionId, Guid projectId, Guid userId);
        Task SaveChangesAsync();
    }
}