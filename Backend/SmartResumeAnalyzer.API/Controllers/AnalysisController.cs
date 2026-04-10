using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.API.Filters;
using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAiAnalysisService _aiAnalysisService;
        private readonly IPdfParserService _pdfParserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;

        public AnalysisController(
            IAiAnalysisService aiAnalysisService,
            IPdfParserService pdfParserService,
            IFileStorageService fileStorageService,
            IProjectService projectService,
            AppDbContext context)
        {
            _aiAnalysisService = aiAnalysisService;
            _pdfParserService = pdfParserService;
            _fileStorageService = fileStorageService;
            _projectService = projectService;
            _context = context;
        }

        [HttpPost("analyze")]
        [ServiceFilter(typeof(RateLimitFilter))]
        public async Task<IActionResult> Analyze([FromForm] AnalysisRequestDto request, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                throw new AppException("CV file is required.");

            if (Path.GetExtension(cvFile.FileName).ToLower() != ".pdf")
                throw new AppException("Only PDF files are allowed.");

            var userId = GetUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            string cvText;
            using (var stream = cvFile.OpenReadStream())
                cvText = _pdfParserService.ExtractText(stream);

            if (string.IsNullOrWhiteSpace(cvText))
                throw new AppException("Could not extract text from the provided PDF. Please ensure the file is not scanned or image-based.");

            var result = await _aiAnalysisService.AnalyzeAsync(
                cvText,
                request.JobTitle,
                request.JobDescription,
                request.SeniorityLevel);

            string storedFileName;
            using (var stream = cvFile.OpenReadStream())
                storedFileName = await _fileStorageService.SaveAsync(stream, cvFile.FileName);

            var log = new Core.Entities.AnalysisLog
            {
                UserId = userId,
                IpAddress = ipAddress,
                JobTitle = request.JobTitle,
                JobDescription = request.JobDescription,
                SeniorityLevel = request.SeniorityLevel,
                OriginalFileName = cvFile.FileName,
                StoredFileName = storedFileName,
                ResultJson = JsonSerializer.Serialize(result)
            };

            await _context.AnalysisLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            result.AnalysisLogId = log.Id;

            if (userId.HasValue)
            {
                Guid projectId;
                using (var stream = cvFile.OpenReadStream())
                {
                    projectId = await _projectService.SaveAnalysisAsProjectAsync(
                        result, request.JobTitle, request.JobDescription, request.SeniorityLevel,
                        stream, cvFile.FileName, userId.Value);
                }

                result.ProjectId = projectId;
            }

            return Ok(result);
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;

            return null;
        }
    }
}