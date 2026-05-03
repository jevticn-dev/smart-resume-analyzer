namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendApplicationEmailAsync(
            string toEmail,
            string fromUserEmail,
            string fromUserName,
            string subject,
            string body,
            byte[] cvBytes,
            string cvFileName
        );
    }
}