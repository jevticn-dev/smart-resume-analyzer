using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.DTOs.Email;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class AiEmailService : IAiEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;

        public AiEmailService(HttpClient httpClient, IOptions<GroqSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<EmailDraftDto> GenerateEmailDraftAsync(
            string jobTitle,
            string companyName,
            string jobDescription,
            string cvText,
            string userName)
        {
            var prompt = BuildPrompt(jobTitle, companyName, jobDescription, cvText, userName);

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.4,
                max_tokens = 1000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                    throw new RateLimitExceededException("AI service rate limit reached. Please try again later.");

                throw new AppException("AI email generation service is currently unavailable.", 503);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseJson);
        }

        private static string BuildPrompt(
            string jobTitle,
            string companyName,
            string jobDescription,
            string cvText,
            string userName)
        {
            return $"""
                Generate a professional job application email in HTML format.
                Return ONLY a valid JSON object with "subject" (plain text) and "body" (HTML) fields. No markdown, no backticks.
                Candidate name: {userName}
                Position: {jobTitle} at {companyName}
                Job description: {jobDescription}
                CV content: {cvText}
                The body should use <p> and <br> tags. Include a greeting, 2-3 paragraphs highlighting relevant experience, and a professional closing with the candidate's name.
                """;
        }

        private static EmailDraftDto ParseResponse(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var messageContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                var cleanJson = messageContent.Trim();
                if (cleanJson.StartsWith("```"))
                {
                    cleanJson = cleanJson
                        .Replace("```json", "")
                        .Replace("```", "")
                        .Trim();
                }

                return JsonSerializer.Deserialize<EmailDraftDto>(cleanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new EmailDraftDto();
            }
            catch (Exception ex) when (ex is not AppException)
            {
                throw new AppException("Failed to parse AI email response. Please try again.");
            }
        }
    }
}