namespace Cessa.Application.Features.Auth
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using System.Threading.Tasks;

    public record LogoutCommand(string RefreshToken) : IRequest<bool>;

    public class LogoutCommandHandler(IApplicationDbContext context) : IRequestHandler<LogoutCommand, bool>
    {
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) 
                return true;

            var tokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                await context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
