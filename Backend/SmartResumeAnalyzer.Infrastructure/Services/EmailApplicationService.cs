using SmartResumeAnalyzer.Core.DTOs.Email;
using SmartResumeAnalyzer.Core.DTOs.Project;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class EmailApplicationService : IEmailApplicationService
    {
        private readonly IAiEmailService _aiEmailService;
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPdfParserService _pdfParserService;
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;

        public EmailApplicationService(
            IAiEmailService aiEmailService,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            IPdfParserService pdfParserService,
            IProjectService projectService,
            AppDbContext context)
        {
            _aiEmailService = aiEmailService;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
            _pdfParserService = pdfParserService;
            _projectService = projectService;
            _context = context;
        }

        public async Task<EmailDraftDto> GenerateDraftAsync(Guid projectId, Guid versionId, Guid userId, string userName)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId)
                ?? throw new NotFoundException("Project not found.");

            var version = await _context.CvVersions
                .FirstOrDefaultAsync(v => v.Id == versionId && v.ProjectId == projectId)
                ?? throw new NotFoundException("CV version not found.");

            var cvBytes = await _fileStorageService.ReadAsync(version.StoredFileName);
            using var stream = new MemoryStream(cvBytes);
            var cvText = _pdfParserService.ExtractText(stream);

            return await _aiEmailService.GenerateEmailDraftAsync(
                project.JobTitle,
                project.CompanyName,
                project.JobDescription,
                cvText,
                userName);
        }

        public async Task<object> SendAsync(SendEmailRequestDto dto, Guid userId, string userName, string userEmail)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.UserId == userId)
                ?? throw new NotFoundException("Project not found.");

            var version = await _context.CvVersions
                .FirstOrDefaultAsync(v => v.Id == dto.VersionId && v.ProjectId == dto.ProjectId)
                ?? throw new NotFoundException("CV version not found.");

            var cvBytes = await _fileStorageService.ReadAsync(version.StoredFileName);

            await _emailService.SendApplicationEmailAsync(
                dto.ToEmail,
                userEmail,
                userName,
                dto.Subject,
                dto.Body,
                cvBytes,
                version.OriginalFileName);

            if (project.Status == "Draft")
            {
                project.Status = "Sent";
                project.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return await _projectService.GetProjectDetailAsync(project.Id, userId);
        }
    }
}