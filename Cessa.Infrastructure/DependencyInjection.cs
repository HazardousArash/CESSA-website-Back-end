using Microsoft.Extensions.DependencyInjection;
using Cessa.Infrastructure.Services.Messaging;
using Cessa.Infrastructure.Services.Storage;
using Cessa.Infrastructure.Services.Auth;
using Cessa.Application.Common.Interfaces;

namespace Cessa.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<ISmsSender, KavenegarSmsService>();

            // Register Resend
            services.AddHttpClient<IEmailSender, ResendEmailService>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            return services;
        }
    }
}