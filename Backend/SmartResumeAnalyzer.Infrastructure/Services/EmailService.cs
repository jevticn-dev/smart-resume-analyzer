using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Core.Settings;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridSettings _settings;

        public EmailService(IOptions<SendGridSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendApplicationEmailAsync(
            string toEmail,
            string fromUserEmail,
            string fromUserName,
            string subject,
            string body,
            byte[] cvBytes,
            string cvFileName)
        {
            var client = new SendGridClient(_settings.ApiKey);

            var from = new EmailAddress(_settings.FromEmail, fromUserName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);
            msg.ReplyTo = new EmailAddress(fromUserEmail);
            msg.AddAttachment(cvFileName, Convert.ToBase64String(cvBytes), "application/pdf");

            var response = await client.SendEmailAsync(msg);

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                throw new AppException("Failed to send email.");
        }
    }
}