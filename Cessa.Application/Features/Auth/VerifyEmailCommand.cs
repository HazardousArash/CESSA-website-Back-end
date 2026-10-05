namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Exceptions;
    using System;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public record VerifyEmailCommand(Guid UserId, string Token) : IRequest<bool>;

    public class VerifyEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<VerifyEmailCommand, bool>
    {
        public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            // 1. If already confirmed (e.g., duplicate request or page refresh), succeed immediately!
            if (user.EmailConfirmed)
            {
                return true;
            }

            // 2. Decode the token (safely handle base64url or raw string)
            string decodedToken;
            try
            {
                var bytes = WebEncoders.Base64UrlDecode(request.Token);
                decodedToken = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                decodedToken = request.Token;
            }

            // 3. Confirm email with decoded token
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                // Fallback: Try with raw token in case it was not Base64Url-encoded
                result = await userManager.ConfirmEmailAsync(user, request.Token);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Token", new[] { $"تایید ایمیل انجام نشد: {errors}" } }
                });
            }

            return true;
        }
    }
}