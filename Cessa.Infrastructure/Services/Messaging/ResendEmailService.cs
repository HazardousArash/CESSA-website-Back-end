namespace Cessa.Infrastructure.Services.Messaging
{
    using Cessa.Application.Common.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class ResendEmailService(HttpClient httpClient, IConfiguration config, ILogger<ResendEmailService> logger) : IEmailSender
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var apiKey = config["Resend:ApiKey"] ?? config["ResendApiKey"];
            var fromEmail = config["Resend:FromEmail"] ?? config["Resend:From"] ?? "noreply@cessaland.ir";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogWarning("Resend API key is missing. Email to {ToEmail} was not sent.", toEmail);
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

            // Resend API expects 'to' as an array of recipient strings
            var payload = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = subject,
                html = body
            };

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Resend failed. Status: {StatusCode}, Error: {Error}", response.StatusCode, responseContent);
                throw new InvalidOperationException($"Resend email error ({response.StatusCode}): {responseContent}");
            }
        }
    }
}