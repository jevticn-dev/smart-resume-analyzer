using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SmartResumeAnalyzer.Core.DTOs.Project;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Configuration;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;
        private readonly string _basePath;

        public ProjectController(IProjectService projectService, AppDbContext context, IOptions<FileStorageSettings> fileStorageSettings)
        {
            _projectService = projectService;
            _context = context;
            _basePath = fileStorageSettings.Value.BasePath;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var userId = GetUserId();
            var projects = await _projectService.GetProjectsAsync(userId);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var userId = GetUserId();
            var project = await _projectService.GetProjectDetailAsync(id, userId);
            return Ok(project);
        }

        [HttpGet("{id}/cv")]
        public async Task<IActionResult> GetCv(Guid id)
        {
            var userId = GetUserId();

            var cvVersion = await _context.CvVersions
                .Include(cv => cv.Project)
                .FirstOrDefaultAsync(cv => cv.Project.Id == id && cv.Project.UserId == userId)
                ?? throw new NotFoundException("CV not found.");

            var filePath = Path.Combine(_basePath, cvVersion.StoredFileName);
            if (!System.IO.File.Exists(filePath))
                throw new NotFoundException("CV file not found.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(cvVersion.OriginalFileName, out var contentType))
                contentType = "application/octet-stream";

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, cvVersion.OriginalFileName);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            var userId = GetUserId();
            var project = await _projectService.CreateProjectAsync(dto, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var userId = GetUserId();
            await _projectService.DeleteProjectAsync(id, userId);
            return NoContent();
        }

        [HttpPost("from-guest-analysis")]
        public async Task<IActionResult> ConvertGuestAnalysis([FromBody] ConvertGuestAnalysisDto dto)
        {
            var userId = GetUserId();
            var project = await _projectService.ConvertGuestAnalysisAsync(dto.AnalysisLogId, userId);
            return Ok(project);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;

            throw new UnauthorizedException("User not authenticated.");
        }
    }
}