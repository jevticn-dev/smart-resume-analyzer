namespace SmartResumeAnalyzer.Core.Entities
{
    public class CvVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public int VersionNumber { get; set; }
        public string OriginalFileName { get; set; } = String.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;
        public Analysis? Analysis { get; set; }
    }
}