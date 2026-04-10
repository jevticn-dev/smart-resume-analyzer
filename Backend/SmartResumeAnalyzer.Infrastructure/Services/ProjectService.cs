using Microsoft.AspNetCore.Http;
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
                return new ProjectSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    JobTitle = p.JobTitle,
                    CompanyName = p.CompanyName,
                    Seniority = p.Seniority,
                    Status = p.Status,
                    LatestMatchScore = latestVersion?.Analysis?.MatchScore,
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

        public async Task<Guid> SaveAnalysisAsProjectAsync(AnalysisResultDto result, string jobTitle, string jobDescription, string seniorityLevel, Stream cvStream, string originalFileName, Guid userId)
        {
            var storedFileName = await _fileStorageService.SaveAsync(cvStream, originalFileName);

            var project = new Project
            {
                UserId = userId,
                Title = jobTitle,
                JobTitle = jobTitle,
                JobDescription = jobDescription,
                Seniority = seniorityLevel
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            var cvVersion = new CvVersion
            {
                ProjectId = project.Id,
                VersionNumber = 1,
                OriginalFileName = originalFileName,
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
                JobDescription = log.JobDescription,
                Seniority = log.SeniorityLevel
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            var cvVersion = new CvVersion
            {
                ProjectId = project.Id,
                VersionNumber = 1,
                OriginalFileName = log.OriginalFileName,
                StoredFileName = log.StoredFileName
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