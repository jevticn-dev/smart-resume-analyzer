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
        private readonly IFileStorageService _fileStorageService;
        private readonly AppDbContext _context;
        private readonly string _basePath;

        public ProjectController(
            IProjectService projectService,
            IFileStorageService fileStorageService,
            AppDbContext context,
            IOptions<FileStorageSettings> fileStorageSettings)
        {
            _projectService = projectService;
            _fileStorageService = fileStorageService;
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

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            var userId = GetUserId();
            var project = await _projectService.CreateProjectAsync(dto, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
        {
            var userId = GetUserId();
            var project = await _projectService.UpdateProjectAsync(id, dto, userId);
            return Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var userId = GetUserId();
            await _projectService.DeleteProjectAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/versions")]
        public async Task<IActionResult> AddCvVersion(Guid id, [FromForm] AddCvVersionDto dto, IFormFile cvFile)
        {
            var userId = GetUserId();

            if (cvFile == null || cvFile.Length == 0)
                return BadRequest("CV file is required.");

            using var memoryStream = new MemoryStream();
            await cvFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var storedFileName = await _fileStorageService.SaveAsync(new MemoryStream(fileBytes), cvFile.FileName);

            var project = await _projectService.AddCvVersionAsync(
                id,
                new MemoryStream(fileBytes),
                cvFile.FileName,
                storedFileName,
                dto.Notes,
                userId
            );

            return Ok(project);
        }

        [HttpGet("{id}/versions/{versionId}/cv")]
        public async Task<IActionResult> GetVersionCv(Guid id, Guid versionId)
        {
            var userId = GetUserId();

            var cvVersion = await _context.CvVersions
                .Include(cv => cv.Project)
                .FirstOrDefaultAsync(cv =>
                    cv.Id == versionId &&
                    cv.ProjectId == id &&
                    cv.Project.UserId == userId)
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

        [HttpGet("{id}/compare")]
        public async Task<IActionResult> CompareVersions(Guid id, [FromQuery] Guid versionAId, [FromQuery] Guid versionBId)
        {
            var userId = GetUserId();
            var result = await _projectService.CompareVersionsAsync(id, versionAId, versionBId, userId);
            return Ok(result);
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