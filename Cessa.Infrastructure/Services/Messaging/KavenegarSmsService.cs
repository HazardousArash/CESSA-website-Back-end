using Cessa.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Cessa.Infrastructure.Services.Messaging
{
    public class KavenegarSmsService(ILogger<KavenegarSmsService> logger) : ISmsSender
    {
        // Fixed CS0414: Commented out unused field to keep a clean build
        // private readonly string _apiKey = "YOUR_KAVENEGAR_API_KEY";

        public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("KAVENEGAR STUB: Sending SMS to {PhoneNumber}. Message: {Message}", phoneNumber, message);
            await Task.CompletedTask;
        }
    }

    public class SmtpEmailService(ILogger<SmtpEmailService> logger) : IEmailSender
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var senderEmail = "no-reply@cessa.iut.ac.ir";
            logger.LogInformation("SMTP STUB: Sending Email from {Sender} to {ToEmail}. Subject: {Subject}", senderEmail, toEmail, subject);
            await Task.CompletedTask;
        }
    }
}
