using Microsoft.AspNetCore.Mvc.Filters;
using SmartResumeAnalyzer.Core.Interfaces;
using System.Security.Claims;
using SmartResumeAnalyzer.Core.Exceptions;

namespace SmartResumeAnalyzer.API.Filters
{
    public class RateLimitFilter : IAsyncActionFilter
    {
        private readonly IRateLimitService _rateLimitService;

        public RateLimitFilter(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = GetUserId(context.HttpContext);
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

            if (!await _rateLimitService.IsAllowedAsync(userId, ipAddress)) 
            {
                throw new AppException("Daily analysis limit reached. Please try again tomorrow.", 429);
            }

            await next();

            await _rateLimitService.RecordCallAsync(userId, ipAddress);
        }

        private static Guid? GetUserId(HttpContext context) 
        {
            var claim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst("sub");

            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}
