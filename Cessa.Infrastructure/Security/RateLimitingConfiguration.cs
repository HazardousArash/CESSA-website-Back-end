namespace Cessa.Api.Infrastructure.Security
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using System.Threading.RateLimiting;

    public static class RateLimitingConfiguration
    {
        public static IServiceCollection AddCessaRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync("{\"status\":429,\"success\":false,\"message\":\"Too many requests.\"}", token);
                };

                // محدودیت فقط برای صفحات حساس لاگین و رمز عبور
                options.AddPolicy("auth-policy", context =>
                {
                    var remoteIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                                ?? context.Connection.RemoteIpAddress?.ToString() 
                                ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: remoteIp,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 30,
                            QueueLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });

            return services;
        }
    }
}
