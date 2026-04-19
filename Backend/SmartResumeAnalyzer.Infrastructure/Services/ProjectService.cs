using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.DTOs.Project;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using System.Text.Json;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly AppDbContext _context;

        public ProjectService(IProjectRepository projectRepository, IFileStorageService fileStorageService, AppDbContext context)
        {
            _projectRepository = projectRepository;
            _fileStorageService = fileStorageService;
            _context = context;
        }

        public async Task<List<ProjectSummaryDto>> GetProjectsAsync(Guid userId)
        {
            var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);

            return projects.Select(p =>
            {
                var latestVersion = p.CvVersions.OrderByDescending(cv => cv.VersionNumber).FirstOrDefault();
                var analysis = latestVersion?.Analysis;

                int? strengthsCount = null;
                int? weaknessesCount = null;
                int? missingKeywordsCount = null;
                int? highCount = null;
                int? mediumCount = null;
                int? lowCount = null;

                if (analysis != null)
                {
                    var strengths = JsonSerializer.Deserialize<List<string>>(analysis.StrengthsJson) ?? [];
                    var weaknesses = JsonSerializer.Deserialize<List<string>>(analysis.WeaknessesJson) ?? [];
                    var missingKeywords = JsonSerializer.Deserialize<List<string>>(analysis.MissingKeywordsJson) ?? [];
                    var suggestions = JsonSerializer.Deserialize<List<SuggestionDto>>(analysis.SuggestionsJson) ?? [];

                    strengthsCount = strengths.Count;
                    weaknessesCount = weaknesses.Count;
                    missingKeywordsCount = missingKeywords.Count;
                    highCount = suggestions.Count(s => s.Priority == "high");
                    mediumCount = suggestions.Count(s => s.Priority == "medium");
                    lowCount = suggestions.Count(s => s.Priority == "low");
                }

                return new ProjectSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    JobTitle = p.JobTitle,
                    CompanyName = p.CompanyName,
                    Seniority = p.Seniority,
                    Status = p.Status,
                    LatestMatchScore = analysis?.MatchScore,
                    StrengthsCount = strengthsCount,
                    WeaknessesCount = weaknessesCount,
                    MissingKeywordsCount = missingKeywordsCount,
                    HighSuggestionsCount = highCount,
                    MediumSuggestionsCount = mediumCount,
                    LowSuggestionsCount = lowCount,
                    CreatedAt = p.CreatedAt
                };
            }).ToList();
        }

        public async Task<ProjectDetailDto> GetProjectDetailAsync(Guid projectId, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            var cvVersion = project.CvVersions.OrderByDescending(cv => cv.VersionNumber).FirstOrDefault();

            return MapToDetailDto(project, cvVersion);
        }

        public async Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto dto, Guid userId)
        {
            var project = new Project
            {
                UserId = userId,
                Title = dto.Title,
                JobTitle = dto.JobTitle,
                JobDescription = dto.JobDescription,
                CompanyName = dto.CompanyName,
                Seniority = dto.SeniorityLevel
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            return MapToDetailDto(project, null);
        }

        public async Task DeleteProjectAsync(Guid projectId, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            foreach (var cv in project.CvVersions)
                _fileStorageService.Delete(cv.StoredFileName);

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task<Guid> SaveAnalysisAsProjectAsync(AnalysisResultDto result, string jobTitle, string companyName, string jobDescription, string seniorityLevel, string storedFileName, string originalFileName, Guid userId, Guid? existingProjectId = null)
        {
            Project project;

            if (existingProjectId.HasValue)
            {
                project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == existingProjectId.Value && p.UserId == userId)
                    ?? throw new NotFoundException("Project not found.");
            }
            else
            {
                project = new Project
                {
                    UserId = userId,
                    Title = jobTitle,
                    JobTitle = jobTitle,
                    CompanyName = companyName,
                    JobDescription = jobDescription,
                    Seniority = seniorityLevel
                };
                await _context.Projects.AddAsync(project);
            }

            var cvVersion = new CvVersion
            {
                Project = project,
                VersionNumber = 1,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName
            };

            var analysis = new Analysis
            {
                CvVersion = cvVersion,
                MatchScore = result.MatchScore,
                StrengthsJson = JsonSerializer.Serialize(result.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(result.Weaknesses),
                MissingKeywordsJson = JsonSerializer.Serialize(result.MissingKeywords),
                SuggestionsJson = JsonSerializer.Serialize(result.Suggestions),
                Summary = result.Summary
            };

            await _context.CvVersions.AddAsync(cvVersion);
            await _context.Analyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return project.Id;
        }

        public async Task<ProjectDetailDto> ConvertGuestAnalysisAsync(Guid analysisLogId, Guid userId)
        {
            var log = await _context.AnalysisLogs.FirstOrDefaultAsync(l => l.Id == analysisLogId)
                ?? throw new NotFoundException("Analysis log not found.");

            if (log.IsConverted)
                throw new AppException("This analysis has already been saved.");

            var result = JsonSerializer.Deserialize<AnalysisResultDto>(log.ResultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new AppException("Failed to read analysis data.");

            var project = new Project
            {
                UserId = userId,
                Title = log.JobTitle,
                JobTitle = log.JobTitle,
                CompanyName = log.CompanyName,
                JobDescription = log.JobDescription,
                Seniority = log.SeniorityLevel
            };

            var cvVersion = new CvVersion
            {
                Project = project,
                VersionNumber = 1,
                OriginalFileName = log.OriginalFileName,
                StoredFileName = log.StoredFileName
            };

            var analysis = new Analysis
            {
                CvVersion = cvVersion,
                MatchScore = result.MatchScore,
                StrengthsJson = JsonSerializer.Serialize(result.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(result.Weaknesses),
                MissingKeywordsJson = JsonSerializer.Serialize(result.MissingKeywords),
                SuggestionsJson = JsonSerializer.Serialize(result.Suggestions),
                Summary = result.Summary
            };

            await _context.Projects.AddAsync(project);
            await _context.CvVersions.AddAsync(cvVersion);
            await _context.Analyses.AddAsync(analysis);

            log.IsConverted = true;
            await _context.SaveChangesAsync();

            return MapToDetailDto(project, cvVersion, analysis, result);
        }

        private static ProjectDetailDto MapToDetailDto(Project project, CvVersion? cvVersion, Analysis? analysis = null, AnalysisResultDto? analysisDto = null)
        {
            AnalysisResultDto? analysisResult = null;

            if (cvVersion?.Analysis != null || analysisDto != null)
            {
                var a = analysis ?? cvVersion?.Analysis;
                analysisResult = analysisDto ?? (a == null ? null : new AnalysisResultDto
                {
                    MatchScore = a.MatchScore,
                    Strengths = JsonSerializer.Deserialize<List<string>>(a.StrengthsJson) ?? [],
                    Weaknesses = JsonSerializer.Deserialize<List<string>>(a.WeaknessesJson) ?? [],
                    MissingKeywords = JsonSerializer.Deserialize<List<string>>(a.MissingKeywordsJson) ?? [],
                    Suggestions = JsonSerializer.Deserialize<List<SuggestionDto>>(a.SuggestionsJson) ?? [],
                    Summary = a.Summary
                });
            }

            return new ProjectDetailDto
            {
                Id = project.Id,
                Title = project.Title,
                JobTitle = project.JobTitle,
                JobDescription = project.JobDescription,
                CompanyName = project.CompanyName,
                Seniority = project.Seniority,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                CvVersion = cvVersion == null ? null : new CvVersionDto
                {
                    Id = cvVersion.Id,
                    OriginalFileName = cvVersion.OriginalFileName,
                    VersionNumber = cvVersion.VersionNumber,
                    CreatedAt = cvVersion.CreatedAt
                },
                Analysis = analysisResult
            };
        }
    }
}