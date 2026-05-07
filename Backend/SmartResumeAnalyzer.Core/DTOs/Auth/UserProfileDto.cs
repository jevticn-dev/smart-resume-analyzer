namespace SmartResumeAnalyzer.Core.DTOs.Auth
{
    public class UserProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int ReminderIntervalDays { get; set; }
        public UserStatsDto Stats { get; set; } = new();
    }

    public class UserStatsDto
    {
        public int TotalProjects { get; set; }
        public int TotalCvVersions { get; set; }
        public int TotalAnalyses { get; set; }
        public int SentApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int DeclinedApplications { get; set; }
        public double AverageMatchScore { get; set; }
        public int RemainingAnalysesToday { get; set; }
    }
}