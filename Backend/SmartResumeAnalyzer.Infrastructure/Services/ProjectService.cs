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
        private readonly IAiAnalysisService _aiAnalysisService;
        private readonly IPdfParserService _pdfParserService;
        private readonly AppDbContext _context;

        public ProjectService(
            IProjectRepository projectRepository,
            IFileStorageService fileStorageService,
            IAiAnalysisService aiAnalysisService,
            IPdfParserService pdfParserService,
            AppDbContext context)
        {
            _projectRepository = projectRepository;
            _fileStorageService = fileStorageService;
            _aiAnalysisService = aiAnalysisService;
            _pdfParserService = pdfParserService;
            _context = context;
        }

        public async Task<List<ProjectSummaryDto>> GetProjectsAsync(Guid userId)
        {
            var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);

            return projects.Select(p =>
            {
                var bestScore = p.CvVersions
                    .Where(cv => cv.Analysis != null)
                    .Select(cv => cv.Analysis!.MatchScore)
                    .DefaultIfEmpty(0)
                    .Max();

                var hasBestScore = p.CvVersions.Any(cv => cv.Analysis != null);

                return new ProjectSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    JobTitle = p.JobTitle,
                    CompanyName = p.CompanyName,
                    Industry = p.Industry,
                    Seniority = p.Seniority,
                    Status = p.Status,
                    VersionCount = p.CvVersions.Count,
                    BestMatchScore = hasBestScore ? bestScore : null,
                    CreatedAt = p.CreatedAt
                };
            }).ToList();
        }

        public async Task<ProjectDetailDto> GetProjectDetailAsync(Guid projectId, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithAllVersionsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            return MapToDetailDto(project);
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
                Industry = dto.Industry,
                CompanyEmail = dto.CompanyEmail,
                Seniority = dto.SeniorityLevel
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            return MapToDetailDto(project);
        }

        public async Task<ProjectDetailDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithAllVersionsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            project.Title = dto.Title;
            project.JobTitle = dto.JobTitle;
            project.CompanyName = dto.CompanyName;
            project.Industry = dto.Industry;
            project.Seniority = dto.SeniorityLevel;
            project.JobDescription = dto.JobDescription;
            project.Status = dto.Status;
            project.CompanyEmail = dto.CompanyEmail;

            await _projectRepository.SaveChangesAsync();

            return MapToDetailDto(project);
        }

        public async Task<ProjectDetailDto> AddCvVersionAsync(Guid projectId, Stream cvStream, string originalFileName, string storedFileName, string notes, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithAllVersionsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            var nextVersionNumber = project.CvVersions.Count > 0
                ? project.CvVersions.Max(v => v.VersionNumber) + 1
                : 1;

            var cvText = _pdfParserService.ExtractText(cvStream);

            var analysisResult = await _aiAnalysisService.AnalyzeAsync(
                cvText,
                project.JobTitle,
                project.CompanyName,
                project.JobDescription,
                project.Seniority
            );

            var cvVersion = new CvVersion
            {
                ProjectId = projectId,
                VersionNumber = nextVersionNumber,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                Notes = notes
            };

            var analysis = new Analysis
            {
                CvVersion = cvVersion,
                MatchScore = analysisResult.MatchScore,
                StrengthsJson = JsonSerializer.Serialize(analysisResult.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(analysisResult.Weaknesses),
                MissingKeywordsJson = JsonSerializer.Serialize(analysisResult.MissingKeywords),
                SuggestionsJson = JsonSerializer.Serialize(analysisResult.Suggestions),
                Summary = analysisResult.Summary
            };

            await _context.CvVersions.AddAsync(cvVersion);
            await _context.Analyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            project.CvVersions.Add(cvVersion);

            return MapToDetailDto(project);
        }

        public async Task<CompareVersionsDto> CompareVersionsAsync(Guid projectId, Guid versionAId, Guid versionBId, Guid userId)
        {
            var project = await _projectRepository.GetProjectWithAllVersionsAsync(projectId, userId)
                ?? throw new NotFoundException("Project not found.");

            var versionA = project.CvVersions.FirstOrDefault(v => v.Id == versionAId)
                ?? throw new NotFoundException("Version A not found.");

            var versionB = project.CvVersions.FirstOrDefault(v => v.Id == versionBId)
                ?? throw new NotFoundException("Version B not found.");

            return new CompareVersionsDto
            {
                VersionA = MapToVersionDetailDto(versionA),
                VersionB = MapToVersionDetailDto(versionB)
            };
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

        public async Task<Guid> SaveAnalysisAsProjectAsync(AnalysisResultDto result, string jobTitle, string companyName, string jobDescription, string seniorityLevel, string industry, string storedFileName, string originalFileName, Guid userId, Guid? existingProjectId = null)
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
                    Seniority = seniorityLevel,
                    Industry = industry
                };

                await _context.Projects.AddAsync(project);
            }

            var nextVersionNumber = await _context.CvVersions
                .Where(cv => cv.ProjectId == project.Id)
                .CountAsync() + 1;

            var cvVersion = new CvVersion
            {
                Project = project,
                VersionNumber = nextVersionNumber,
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

            cvVersion.Analysis = analysis;
            project.CvVersions = [cvVersion];

            return MapToDetailDto(project);
        }

        private static ProjectDetailDto MapToDetailDto(Project project)
        {
            return new ProjectDetailDto
            {
                Id = project.Id,
                Title = project.Title,
                JobTitle = project.JobTitle,
                JobDescription = project.JobDescription,
                CompanyName = project.CompanyName,
                CompanyEmail = project.CompanyEmail,
                Industry = project.Industry,
                Seniority = project.Seniority,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                CvVersions = project.CvVersions
                    .OrderByDescending(v => v.VersionNumber)
                    .Select(MapToVersionDetailDto)
                    .ToList()
            };
        }

        private static CvVersionDetailDto MapToVersionDetailDto(CvVersion v)
        {
            return new CvVersionDetailDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                OriginalFileName = v.OriginalFileName,
                Notes = v.Notes,
                CreatedAt = v.CreatedAt,
                Analysis = v.Analysis == null ? null : new AnalysisResultDto
                {
                    MatchScore = v.Analysis.MatchScore,
                    Strengths = JsonSerializer.Deserialize<List<string>>(v.Analysis.StrengthsJson) ?? [],
                    Weaknesses = JsonSerializer.Deserialize<List<string>>(v.Analysis.WeaknessesJson) ?? [],
                    MissingKeywords = JsonSerializer.Deserialize<List<string>>(v.Analysis.MissingKeywordsJson) ?? [],
                    Suggestions = JsonSerializer.Deserialize<List<SuggestionDto>>(v.Analysis.SuggestionsJson) ?? [],
                    Summary = v.Analysis.Summary
                }
            };
        }
    }
}