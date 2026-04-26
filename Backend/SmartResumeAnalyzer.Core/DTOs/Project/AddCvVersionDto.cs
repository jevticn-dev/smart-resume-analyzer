using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class AddCvVersionDto
    {
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}