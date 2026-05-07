using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Core.Settings;

namespace SmartResumeAnalyzer.Infrastructure.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationSettings _settings;

        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<NotificationSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_settings.CheckIntervalMinutes), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.GenerateNotificationsAsync();
            }
        }
    }
}