using System.ComponentModel.DataAnnotations;

namespace SmartResumeAnalyzer.Core.DTOs.Email
{
    public class GenerateEmailDraftRequestDto
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid VersionId { get; set; }
    }
}