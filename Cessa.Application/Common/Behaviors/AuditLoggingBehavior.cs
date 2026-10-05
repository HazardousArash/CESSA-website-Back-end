namespace Cessa.Application.Common.Behaviors
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Domain.Entities.Admin;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text.Json;
    using System.Security.Claims;

    public class AuditLoggingBehavior<TRequest, TResponse>(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();
            var requestName = typeof(TRequest).Name;

            bool isSensitiveCommand = 
                requestName.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
                requestName.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                requestName.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                requestName.Contains("Register", StringComparison.OrdinalIgnoreCase) ||
                requestName.Contains("Otp", StringComparison.OrdinalIgnoreCase) ||
                requestName.Contains("Bypass", StringComparison.OrdinalIgnoreCase);

            if (requestName.EndsWith("Command") && !isSensitiveCommand)
            {
                var email = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? "System/Public";
                var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";

                string payload = string.Empty;
                try
                {
                    payload = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = false });
                    if (payload.Length > 800) payload = payload.Substring(0, 800) + "...";
                }
                catch { payload = "[Unserializable Payload]"; }

                context.AuditLogs.Add(new AuditLog
                {
                    Action = requestName.Replace("Command", ""),
                    Details = $"UserEmail: {email} | UserId: {userId} | Payload: {payload}",
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync(cancellationToken);
            }
            return response;
        }
    }
}
