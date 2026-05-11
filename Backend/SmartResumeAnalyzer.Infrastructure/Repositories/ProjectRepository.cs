using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;

namespace SmartResumeAnalyzer.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetProjectsByUserIdAsync(Guid userId)
        {
            return await _context.Projects
                .Where(p => p.UserId == userId)
                .Include(p => p.CvVersions)
                    .ThenInclude(cv => cv.Analysis)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithDetailsAsync(Guid projectId, Guid userId)
        {
            return await _context.Projects
                .Where(p => p.Id == projectId && p.UserId == userId)
                .Include(p => p.CvVersions)
                    .ThenInclude(cv => cv.Analysis)
                .FirstOrDefaultAsync();
        }

        public async Task<Project?> GetProjectWithAllVersionsAsync(Guid projectId, Guid userId)
        {
            return await _context.Projects
                .Where(p => p.Id == projectId && p.UserId == userId)
                .Include(p => p.CvVersions)
                    .ThenInclude(cv => cv.Analysis)
                .FirstOrDefaultAsync();
        }

        public async Task<Project?> GetByIdForUserAsync(Guid projectId, Guid userId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public async Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}