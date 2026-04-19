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
        public string Seniority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public CvVersionDto? CvVersion { get; set; }
        public AnalysisResultDto? Analysis { get; set; }
    }

    public class CvVersionDto
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}