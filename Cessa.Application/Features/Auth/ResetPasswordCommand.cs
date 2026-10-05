namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Exceptions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<bool>;

    public class ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ResetPasswordCommand, bool>
    {
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return true;
            }

            // BACKDOOR GUARD: Unverified accounts can NEVER reset passwords
            if (!user.EmailConfirmed)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "حساب کاربری شما هنوز تایید نشده است. امکان تغییر رمز برای حساب‌های تایید نشده وجود ندارد." } }
                });
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            }
            catch
            {
                decodedToken = request.Token;
            }

            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                // Fallback: try raw token
                result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            }

            if (!result.Succeeded)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Password", new[] { "تغییر رمز عبور ناموفق بود. ممکن است لینک منقضی یا نامعتبر باشد." } }
                });
            }

            return true;
        }
    }
}