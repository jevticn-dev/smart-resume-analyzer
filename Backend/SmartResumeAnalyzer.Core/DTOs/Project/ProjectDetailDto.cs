using SmartResumeAnalyzer.Core.DTOs.Analysis;

namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class ProjectDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CvVersionDetailDto> CvVersions { get; set; } = [];
    }

    public class CvVersionDetailDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AnalysisResultDto? Analysis { get; set; }
    }
}