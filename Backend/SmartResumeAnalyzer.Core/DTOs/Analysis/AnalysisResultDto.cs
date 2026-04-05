namespace SmartResumeAnalyzer.Core.DTOs.Analysis
{
    public class AnalysisResultDto
    {
        public int MatchScore { get; set; }
        public List<string> Strengths { get; set; } = [];
        public List<string> Weaknesses { get; set; } = [];
        public List<string> MissingKeywords { get; set; } = [];
        public List<SuggestionDto> Suggestions { get; set; } = [];
        public string Summary { get; set; } = string.Empty;
    }

    public class SuggestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}