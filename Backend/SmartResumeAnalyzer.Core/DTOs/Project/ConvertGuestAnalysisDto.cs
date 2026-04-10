using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class ConvertGuestAnalysisDto
    {
        [Required]
        public Guid AnalysisLogId { get; set; }
    }
}