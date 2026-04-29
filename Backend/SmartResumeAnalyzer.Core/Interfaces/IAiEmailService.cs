using SmartResumeAnalyzer.Core.DTOs.Email;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAiEmailService
    {
        Task<EmailDraftDto> GenerateEmailDraftAsync(
            string jobTitle,
            string companyName,
            string jobDescription,
            string cvText,
            string userName
        );
    }
}