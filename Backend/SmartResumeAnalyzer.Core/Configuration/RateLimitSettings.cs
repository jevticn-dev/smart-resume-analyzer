namespace SmartResumeAnalyzer.Core.Configuration
{
    public class RateLimitSettings
    {
        public int DailyLimitForGuests { get; set; } = 3;
        public int DailyLimitForUsers { get; set; } = 10;
    }
}
