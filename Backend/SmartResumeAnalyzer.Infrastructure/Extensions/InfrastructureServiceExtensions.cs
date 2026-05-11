using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Core.Settings;
using SmartResumeAnalyzer.Infrastructure.Data;
using SmartResumeAnalyzer.Infrastructure.Repositories;
using SmartResumeAnalyzer.Infrastructure.Services;
using SmartResumeAnalyzer.Infrastructure.BackgroundServices;

namespace SmartResumeAnalyzer.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<RateLimitSettings>(configuration.GetSection("RateLimitSettings"));
            services.Configure<GroqSettings>(configuration.GetSection("GroqSettings"));
            services.Configure<FileStorageSettings>(configuration.GetSection("FileStorageSettings"));
            services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
            services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ICvVersionRepository, CvVersionRepository>();
            services.AddScoped<IAnalysisLogRepository, AnalysisLogRepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRateLimitService, RateLimitService>();
            services.AddScoped<IPdfParserService, PdfParserService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAnalysisLogService, AnalysisLogService>();
            services.AddScoped<IEmailApplicationService, EmailApplicationService>();

            services.AddHostedService<NotificationBackgroundService>();

            services.AddHttpClient<IAiAnalysisService, AiAnalysisService>();
            services.AddHttpClient<IAiEmailService, AiEmailService>();

            return services;
        }
    }
}
