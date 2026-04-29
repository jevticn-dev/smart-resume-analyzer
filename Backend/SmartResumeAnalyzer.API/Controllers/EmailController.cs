using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.API.Filters;
using SmartResumeAnalyzer.Core.DTOs.Email;
using SmartResumeAnalyzer.Core.DTOs.Project;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IAiEmailService _aiEmailService;
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPdfParserService _pdfParserService;
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;

        public EmailController(
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

        [HttpPost("generate-draft")]
        [ServiceFilter(typeof(RateLimitFilter))]
        public async Task<IActionResult> GenerateDraft([FromBody] GenerateEmailDraftRequestDto dto)
        {
            var userId = GetUserId();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.UserId == userId)
                ?? throw new NotFoundException("Project not found.");

            var version = await _context.CvVersions
                .FirstOrDefaultAsync(v => v.Id == dto.VersionId && v.ProjectId == dto.ProjectId)
                ?? throw new NotFoundException("CV version not found.");

            var cvBytes = await _fileStorageService.ReadAsync(version.StoredFileName);
            using var stream = new MemoryStream(cvBytes);
            var cvText = _pdfParserService.ExtractText(stream);

            var draft = await _aiEmailService.GenerateEmailDraftAsync(
                project.JobTitle,
                project.CompanyName,
                project.JobDescription,
                cvText,
                GetUserName()
            );

            return Ok(draft);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto dto)
        {
            var userId = GetUserId();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.UserId == userId)
                ?? throw new NotFoundException("Project not found.");

            var version = await _context.CvVersions
                .FirstOrDefaultAsync(v => v.Id == dto.VersionId && v.ProjectId == dto.ProjectId)
                ?? throw new NotFoundException("CV version not found.");

            var cvBytes = await _fileStorageService.ReadAsync(version.StoredFileName);

            var userName = GetUserName();
            var userEmail = GetUserEmail();

            await _emailService.SendApplicationEmailAsync(
                dto.ToEmail,
                userEmail,
                userName,
                dto.Subject,
                dto.Body,
                cvBytes,
                version.OriginalFileName
            );

            if (project.Status == "Draft")
            {
                project.Status = "Sent";
                project.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var updatedProject = await _projectService.GetProjectDetailAsync(project.Id, userId);
            return Ok(updatedProject);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;

            throw new UnauthorizedException("User not authenticated.");
        }

        private string GetUserName()
        {
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            var fullName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrEmpty(fullName) ? "Applicant" : fullName;
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}