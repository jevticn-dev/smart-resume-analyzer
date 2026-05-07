namespace SmartResumeAnalyzer.Core.DTOs.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}