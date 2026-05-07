using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Auth
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        public int ReminderIntervalDays { get; set; } = 7;
    }
}