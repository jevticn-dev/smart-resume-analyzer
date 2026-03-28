namespace SmartResumeAnalyzer.Core.Entities
{
    public class ApiUsage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public string? IpAddress { get; set; }
        public int CallCount { get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public User? User { get; set; }
    }
}