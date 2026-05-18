using SmartResumeAnalyzer.Core.DTOs.Email;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IEmailApplicationService
    {
        Task<EmailDraftDto> GenerateDraftAsync(Guid projectId, Guid versionId, Guid userId, string userName);
        Task<object> SendAsync(SendEmailRequestDto dto, Guid userId, string userName, string userEmail);
    }
}