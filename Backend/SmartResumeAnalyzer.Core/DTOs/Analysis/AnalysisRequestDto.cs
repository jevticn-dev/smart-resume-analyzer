using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Analysis
{
    public class AnalysisRequestDto
    {
        public Guid? ProjectId { get; set; }

        [Required(ErrorMessage = "Job title is required.")]
        [MaxLength(200, ErrorMessage = "Job title cannot exceed 200 characters.")]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        [MaxLength(5000, ErrorMessage = "Job description cannot exceed 5000 characters.")]
        public string JobDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seniority level is required.")]
        public string SeniorityLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Industry is required.")]
        public string Industry { get; set; } = string.Empty;
    }
}