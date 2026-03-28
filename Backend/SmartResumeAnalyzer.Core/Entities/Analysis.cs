namespace SmartResumeAnalyzer.Core.Entities
{
    public class Analysis
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CvVersionId { get; set; }
        public int MatchScore { get; set; }
        public string StrengthsJson { get; set; } = string.Empty;
        public string WeaknessesJson { get; set; } = string.Empty;
        public string MissingKeywordsJson { get; set; } = string.Empty;
        public string SuggestionsJson { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CvVersion CvVersion { get; set; } = null!;
    }
}