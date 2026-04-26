namespace SmartResumeAnalyzer.Core.DTOs.Project
{
    public class CompareVersionsDto
    {
        public CvVersionDetailDto VersionA { get; set; } = null!;
        public CvVersionDetailDto VersionB { get; set; } = null!;
    }
}