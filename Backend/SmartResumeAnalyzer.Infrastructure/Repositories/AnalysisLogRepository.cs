using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;

namespace SmartResumeAnalyzer.Infrastructure.Repositories
{
    public class AnalysisLogRepository : IAnalysisLogRepository
    {
        private readonly AppDbContext _context;

        public AnalysisLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalysisLog?> GetByIdAsync(Guid id)
        {
            return await _context.AnalysisLogs.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(AnalysisLog log)
        {
            await _context.AnalysisLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}