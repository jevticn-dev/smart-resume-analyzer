using SmartResumeAnalyzer.Core.DTOs.Common;
using SmartResumeAnalyzer.Core.Exceptions;
using System.Text.Json;

namespace SmartResumeAnalyzer.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Application exception: {Message}", ex.Message);
                await WriteErrorResponse(context, ex.StatusCode, ex.Message);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteErrorResponse(context, 500, "An internal server error occurred.");
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message) 
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
