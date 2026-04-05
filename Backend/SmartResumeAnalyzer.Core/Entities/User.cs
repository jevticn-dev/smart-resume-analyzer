namespace SmartResumeAnalyzer.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<ApiUsage> ApiUsages { get; set; } = new List<ApiUsage>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}