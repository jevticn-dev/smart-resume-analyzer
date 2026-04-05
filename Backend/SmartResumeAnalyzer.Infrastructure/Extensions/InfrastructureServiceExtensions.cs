using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using SmartResumeAnalyzer.Infrastructure.Repositories;
using SmartResumeAnalyzer.Infrastructure.Services;

namespace SmartResumeAnalyzer.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<RateLimitSettings>(configuration.GetSection("RateLimitSettings"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRateLimitService, RateLimitService>();

            return services;
        }
    }
}
