using Microsoft.Extensions.Options;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.DTOs.Analysis;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class AiAnalysisService : IAiAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;

        public AiAnalysisService(HttpClient httpClient, IOptions<GroqSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<AnalysisResultDto> AnalyzeAsync(string cvText, string jobTitle, string jobDescription, string seniorityLevel)
        {
            var prompt = BuildPrompt(cvText, jobTitle, jobDescription, seniorityLevel);

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.3,
                max_tokens = 2000
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

                throw new AppException("AI analysis service is currently unavailable.", 503);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseJson);
        }

        private static string BuildPrompt(string cvText, string jobTitle, string jobDescription, string seniorityLevel)
        {
            var jsonStructure = """
                {
                    "matchScore": <integer 0-100>,
                    "strengths": [<list of strings>],
                    "weaknesses": [<list of strings>],
                    "missingKeywords": [<list of strings>],
                    "suggestions": [
                        {
                            "text": <string>,
                            "priority": <"high" | "medium" | "low">
                        }
                    ],
                    "summary": <string, 2-3 sentences>
                }
            """;

            return $"""
                        You are an expert CV analyzer. Analyze the following CV against the job requirements and return ONLY a valid JSON object with no additional text, markdown, or explanation.

                        Job Title: {jobTitle}
                        Seniority Level: {seniorityLevel}
                        Job Description: {jobDescription}

                        CV Content:
                        {cvText}

                        Return this exact JSON structure:
                        {jsonStructure}
                    """;
        }

        private static AnalysisResultDto ParseResponse(string responseJson)
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

                return JsonSerializer.Deserialize<AnalysisResultDto>(cleanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new AnalysisResultDto();
            }
            catch (Exception ex) when (ex is not AppException)
            {
                throw new AppException("Failed to parse AI response. Please try again.");
            }
        }
    }
}