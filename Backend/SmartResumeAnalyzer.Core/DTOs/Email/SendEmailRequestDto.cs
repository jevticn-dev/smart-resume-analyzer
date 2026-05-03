using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Email
{
    public class SendEmailRequestDto
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid VersionId { get; set; }
        [Required]
        [MaxLength(200)]
        public string ToEmail { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
    }
}