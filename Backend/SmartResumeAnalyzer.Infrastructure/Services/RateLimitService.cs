using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly AppDbContext _context;
        private readonly RateLimitSettings _settings;

        public RateLimitService(AppDbContext context, IOptions<RateLimitSettings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }

        public async Task<bool> IsAllowedAsync(Guid? userId, string? ipAddress)
        {
            var today = DateTime.UtcNow.Date;
            var limit = userId.HasValue ? _settings.DailyLimitForUsers : _settings.DailyLimitForGuests;

            var usage = await GetUsageAsync(userId, ipAddress, today);
            return usage == null || usage.CallCount < limit;
        }

        public async Task RecordCallAsync(Guid? userId, string? ipAddress)
        {
            var today = DateTime.UtcNow.Date;
            var usage = await GetUsageAsync(userId, ipAddress, today);

            if (usage == null)
            {
                usage = new ApiUsage
                {
                    UserId = userId,
                    IpAddress = ipAddress,
                    CallCount = 1,
                    Date = today
                };

                await _context.ApiUsage.AddAsync(usage);
            }
            else 
            {
                usage.CallCount++;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ApiUsage?> GetUsageAsync(Guid? userId, string? ipAddress, DateTime date) 
        {
            if (userId.HasValue)
            {
                return await _context.ApiUsage.FirstOrDefaultAsync(u => u.UserId == userId && u.Date == date);
            }

            return await _context.ApiUsage.FirstOrDefaultAsync(u => u.IpAddress == ipAddress && u.Date == date);
        }
    }
}
