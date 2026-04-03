namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IRateLimitService
    {
        Task<bool> IsAllowedAsync(Guid? userId, string? ipAddress);
        Task RecordCallAsync(Guid? userId, string? ipAddress);
    }
}
