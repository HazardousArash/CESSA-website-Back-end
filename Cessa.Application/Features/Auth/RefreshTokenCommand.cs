namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Domain.Entities.Auth;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;

    public record RefreshTokenCommand(string? AccessToken, string RefreshToken) : IRequest<AuthResult>;

    public class RefreshTokenCommandHandler(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<RefreshTokenCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Refresh token is missing" } } });
            }

            var storedRefreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken)
                ?? throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Refresh token does not exist" } } });

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow || storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
            {
                throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Refresh token is invalid, used, or expired" } } });
            }

            // If AccessToken is passed, verify JTI matches
            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                try
                {
                    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(request.AccessToken);
                    if (!string.IsNullOrEmpty(jwtToken?.Id) && storedRefreshToken.JwtId != jwtToken.Id)
                    {
                        throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Token mismatch" } } });
                    }
                }
                catch (ArgumentException)
                {
                    // If accessToken is malformed, proceed using storedRefreshToken.UserId
                }
            }

            storedRefreshToken.IsUsed = true;
            context.RefreshTokens.Update(storedRefreshToken);

            var user = await userManager.FindByIdAsync(storedRefreshToken.UserId.ToString())
                ?? throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "User associated with token no longer exists" } } });

            if (user.IsDeleted)
            {
                throw new ValidationException(new Dictionary<string, string[]> { { "Token", new[] { "User account has been deleted" } } });
            }

            var roles = await userManager.GetRolesAsync(user);

            var newJti = Guid.NewGuid().ToString();
            var newAccessToken = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles, newJti);
            var newRefreshTokenString = jwtTokenGenerator.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenString,
                JwtId = newJti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            context.RefreshTokens.Add(newRefreshToken);
            await context.SaveChangesAsync(cancellationToken);

            return new AuthResult(newAccessToken, newRefreshTokenString);
        }
    }
}