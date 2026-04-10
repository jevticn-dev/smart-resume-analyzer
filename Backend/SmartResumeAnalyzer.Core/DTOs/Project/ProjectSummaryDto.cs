namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class ProjectSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? LatestMatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}