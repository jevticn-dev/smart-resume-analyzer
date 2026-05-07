using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}