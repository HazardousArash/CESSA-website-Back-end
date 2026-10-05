namespace Cessa.Api.Infrastructure
{
    using Cessa.Application.Common.Exceptions;
    using Cessa.Api.Helpers;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            ApiResult<object> response;

            if (exception is ValidationException validationEx)
            {
                response = ApiResult.BadRequest("Validation Failed", validationEx.Errors);
            }
            else if (exception is NotFoundException notFoundEx)
            {
                response = ApiResult.NotFound(notFoundEx.Message);
            }
            else if (exception is UnauthorizedAccessException unauthEx)
            {
                response = ApiResult.Forbidden(unauthEx.Message);
            }
            else
            {
                // SECURITY FIX: Never return raw database/runtime exception messages in Production
                string message = env.IsDevelopment()
                    ? exception.Message
                    : "خطایی در پردازش درخواست شما رخ داده است. لطفاً بعداً تلاش فرمایید.";

                response = ApiResult.Error(message);
            }

            httpContext.Response.StatusCode = response.Status;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
