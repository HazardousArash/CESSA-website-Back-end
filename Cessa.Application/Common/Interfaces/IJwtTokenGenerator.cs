namespace Cessa.Application.Common.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string email, IList<string> roles, string jti);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
