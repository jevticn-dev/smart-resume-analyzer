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
        public int? StrengthsCount { get; set; }
        public int? WeaknessesCount { get; set; }
        public int? MissingKeywordsCount { get; set; }
        public int? HighSuggestionsCount { get; set; }
        public int? MediumSuggestionsCount { get; set; }
        public int? LowSuggestionsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}