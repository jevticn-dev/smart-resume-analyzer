using SmartResumeAnalyzer.Core.Entities;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAnalysisLogRepository
    {
        Task<AnalysisLog?> GetByIdAsync(Guid id);
        Task AddAsync(AnalysisLog log);
        Task SaveChangesAsync();
    }
}