namespace Cessa.Api.Infrastructure.Security
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class SecurityHeadersMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Hardened CSP: removed unsafe-eval
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data: https: http:; " +
                "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "connect-src 'self' http: https: ws: wss:;");

            await next(context);
        }
    }
}
