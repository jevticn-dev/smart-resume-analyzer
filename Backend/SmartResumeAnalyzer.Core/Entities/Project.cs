namespace SmartResumeAnalyzer.Core.Entities
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public int ReminderDays { get; set; } = 7;
        public string CompanyEmail { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public ICollection<CvVersion> CvVersions { get; set; } = new List<CvVersion>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}