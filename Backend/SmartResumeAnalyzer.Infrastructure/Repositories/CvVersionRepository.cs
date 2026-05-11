using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;

namespace SmartResumeAnalyzer.Infrastructure.Repositories
{
    public class CvVersionRepository : ICvVersionRepository
    {
        private readonly AppDbContext _context;

        public CvVersionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CvVersion?> GetByIdWithProjectAsync(Guid versionId, Guid projectId, Guid userId)
        {
            return await _context.CvVersions
                .Include(cv => cv.Project)
                .FirstOrDefaultAsync(cv =>
                    cv.Id == versionId &&
                    cv.ProjectId == projectId &&
                    cv.Project.UserId == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}