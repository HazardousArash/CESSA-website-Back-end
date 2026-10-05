namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record LoginWithPasswordCommand(string Email, string Password, string ExpectedRole) : IRequest<AuthResult>;

    public class LoginWithPasswordCommandHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginWithPasswordCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginWithPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new ValidationException(new Dictionary<string, string[]> { { "Email", new[] { "Invalid credentials." } } });

            if (user.IsDeleted)
                throw new ValidationException(new Dictionary<string, string[]> { { "Email", new[] { "حساب کاربری شما غیرفعال یا حذف شده است." } } });

            if (!user.EmailConfirmed)
                throw new ValidationException(new Dictionary<string, string[]> { { "Email", new[] { "Please verify your email address before logging in." } } });

            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Contains(request.ExpectedRole))
                throw new ValidationException(new Dictionary<string, string[]> { { "Role", new[] { "شما دسترسی لازم برای ورود به این پورتال را ندارید." } } });

            var result = await userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
                throw new ValidationException(new Dictionary<string, string[]> { { "Password", new[] { "Invalid credentials." } } });

            var jti = Guid.NewGuid().ToString();
            var accessToken = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles, jti);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            var tokenEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };
            context.RefreshTokens.Add(tokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            return new AuthResult(accessToken, refreshToken);
        }
    }

    public record LoginWithOtpCommand(string Email, string Code, string ExpectedRole) : IRequest<AuthResult>;

    public class LoginWithOtpCommandHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginWithOtpCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginWithOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new ValidationException(new Dictionary<string, string[]> { { "Email", new[] { "Invalid user." } } });

            if (user.IsDeleted)
                throw new ValidationException(new Dictionary<string, string[]> { { "Email", new[] { "حساب کاربری شما غیرفعال یا حذف شده است." } } });

            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Contains(request.ExpectedRole))
                throw new ValidationException(new Dictionary<string, string[]> { { "Role", new[] { "شما دسترسی لازم برای ورود به این پورتال را ندارید." } } });

            var otp = await context.OtpRecords
                .Where(o => o.UserId == user.Id && o.Code == request.Code && !o.IsUsed && o.ExpirationTime > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ValidationException(new Dictionary<string, string[]> { { "Code", new[] { "Invalid or expired OTP." } } });

            otp.IsUsed = true;

            var jti = Guid.NewGuid().ToString();
            var accessToken = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles, jti);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            var tokenEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };
            context.RefreshTokens.Add(tokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            return new AuthResult(accessToken, refreshToken);
        }
    }
}