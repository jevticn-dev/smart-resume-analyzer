using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.Entities;
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
        private readonly IRateLimitService _rateLimitService;
        private readonly AppDbContext _context;

        public AnalysisController(
            IAiAnalysisService aiAnalysisService,
            IPdfParserService pdfParserService,
            IFileStorageService fileStorageService,
            IRateLimitService rateLimitService,
            AppDbContext context)
        {
            _aiAnalysisService = aiAnalysisService;
            _pdfParserService = pdfParserService;
            _fileStorageService = fileStorageService;
            _rateLimitService = rateLimitService;
            _context = context;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromForm] AnalysisRequestDto request, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                throw new AppException("CV file is required.");

            if (Path.GetExtension(cvFile.FileName).ToLower() != ".pdf")
                throw new AppException("Only PDF files are allowed.");

            var userId = GetUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (!await _rateLimitService.IsAllowedAsync(userId, ipAddress))
                throw new RateLimitExceededException("Daily analysis limit reached. Please try again tomorrow.");

            string cvText;
            using (var stream = cvFile.OpenReadStream())
            {
                cvText = _pdfParserService.ExtractText(stream);
            }

            if (string.IsNullOrWhiteSpace(cvText))
                throw new AppException("Could not extract text from the provided PDF. Please ensure the file is not scanned or image-based.");

            var result = await _aiAnalysisService.AnalyzeAsync(
                cvText,
                request.JobTitle,
                request.JobDescription,
                request.SeniorityLevel);

            await _rateLimitService.RecordCallAsync(userId, ipAddress);

            if (userId.HasValue)
                await SaveAnalysisAsync(result, request, cvFile, userId.Value);

            return Ok(result);
        }

        private async Task SaveAnalysisAsync(AnalysisResultDto result, AnalysisRequestDto request, IFormFile cvFile, Guid userId)
        {
            string storedFileName;
            using (var stream = cvFile.OpenReadStream())
            {
                storedFileName = await _fileStorageService.SaveAsync(stream, cvFile.FileName);
            }

            var project = new Project
            {
                UserId = userId,
                Title = request.JobTitle,
                JobTitle = request.JobTitle,
                JobDescription = request.JobDescription,
                Seniority = request.SeniorityLevel
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var cvVersion = new CvVersion
            {
                ProjectId = project.Id,
                VersionNumber = 1,
                OriginalFileName = cvFile.FileName,
                StoredFileName = storedFileName
            };

            await _context.CvVersions.AddAsync(cvVersion);
            await _context.SaveChangesAsync();

            var analysis = new Analysis
            {
                CvVersionId = cvVersion.Id,
                MatchScore = result.MatchScore,
                StrengthsJson = JsonSerializer.Serialize(result.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(result.Weaknesses),
                MissingKeywordsJson = JsonSerializer.Serialize(result.MissingKeywords),
                SuggestionsJson = JsonSerializer.Serialize(result.Suggestions),
                Summary = result.Summary
            };

            await _context.Analyses.AddAsync(analysis);
            await _context.SaveChangesAsync();
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