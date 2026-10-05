namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Interfaces;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public record ForgotPasswordCommand(string Email) : IRequest<bool>;

    public class ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager, 
        IEmailSender emailSender,
        IConfiguration config,
        ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, bool>
    {
        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            // Strict security: ignore non-existent or unverified accounts
            if (user == null || !user.EmailConfirmed)
            {
                logger.LogWarning("Password reset requested for unverified/missing email: {Email}", request.Email);
                return true;
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Dynamically read frontend URL (falls back to server IP, never localhost)
            var frontendUrl = config["FrontendUrl"] ?? config["AppUrl"] ?? "http://62.60.191.97:3000";
            var resetUrl = $"{frontendUrl.TrimEnd('/')}/reset-password?email={request.Email}&token={encodedToken}";

            try
            {
                await emailSender.SendEmailAsync(
                    request.Email,
                    "Reset your Cessa Password",
                    $"<h1>Password Reset Request</h1><p>You can reset your password by clicking <a href='{resetUrl}'>here</a>.</p><p>If you did not request this, please ignore this email.</p>",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
            }

            return true;
        }
    }
}
