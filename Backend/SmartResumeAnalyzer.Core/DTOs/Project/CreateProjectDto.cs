using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string JobDescription { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string SeniorityLevel { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Industry { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CompanyEmail { get; set; } = string.Empty;
    }
}