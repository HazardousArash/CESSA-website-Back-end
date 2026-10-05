namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Cessa.Domain.Entities.Auth;
    using Microsoft.AspNetCore.Identity;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string Role) : IRequest<string>;

    public class RegisterCommandHandler(
        UserManager<ApplicationUser> userManager, 
        IEmailSender emailSender, 
        IConfiguration config,
        ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, string>
    {
        public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FirstName = request.FirstName, LastName = request.LastName };
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new ValidationException(result.Errors.GroupBy(e => e.Code, e => e.Description).ToDictionary(g => g.Key, g => g.ToArray()));

            await userManager.AddToRoleAsync(user, request.Role);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Dynamically read frontend URL (falls back to server IP, never localhost)
            var frontendUrl = config["FrontendUrl"] ?? config["AppUrl"] ?? "http://62.60.191.97:3000";
            var verifyUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?userId={user.Id}&token={encodedToken}";

            try
            {
                await emailSender.SendEmailAsync(request.Email, "Verify your Cessa account",
                    $"<h1>Welcome to Cessa!</h1><p>Please click <a href='{verifyUrl}'>here</a> to verify your email address and activate your account.</p>", cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send account verification email to {Email}", request.Email);
            }

            return "Registration successful. Please check your email to verify your account before logging in.";
        }
    }
}
