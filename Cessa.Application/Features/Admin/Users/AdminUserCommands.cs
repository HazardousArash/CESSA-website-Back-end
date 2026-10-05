namespace Cessa.Application.Features.Admin.Users
{
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Application.Common.Exceptions;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public record RemoveUserCommand(Guid UserId) : IRequest<bool>;

    public class RemoveUserHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<RemoveUserCommand, bool>
    {
        public async Task<bool> Handle(RemoveUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            user.IsDeleted = true;
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue; // Prevent any future login

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }

    public record BypassUserPasswordCommand(Guid UserId, string NewPassword) : IRequest<bool>;

    public class BypassUserPasswordHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<BypassUserPasswordCommand, bool>
    {
        public async Task<bool> Handle(BypassUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

            return result.Succeeded;
        }
    }

    public record VerifyContentCreatorCommand(Guid UserId) : IRequest<bool>;

    public class VerifyContentCreatorHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<VerifyContentCreatorCommand, bool>
    {
        public async Task<bool> Handle(VerifyContentCreatorCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

            user.IsVerifiedCreator = true;
            await userManager.UpdateAsync(user);
            return true;
        }
    }

    public record WipeOutDeletedUsersCommand() : IRequest<int>;

    public class WipeOutDeletedUsersCommandHandler(IApplicationDbContext context) : IRequestHandler<WipeOutDeletedUsersCommand, int>
    {
        public async Task<int> Handle(WipeOutDeletedUsersCommand request, CancellationToken cancellationToken)
        {
            // 1. Find all soft-deleted user IDs
            var softDeletedUserIds = await context.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            if (softDeletedUserIds.Count == 0)
                return 0;

            var dbContext = context as DbContext;
            using var transaction = dbContext != null ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;

            try
            {
                // 2. Delete child records directly through IApplicationDbContext DbSets
                await context.RefreshTokens
                    .Where(r => softDeletedUserIds.Contains(r.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.OtpRecords
                    .Where(o => softDeletedUserIds.Contains(o.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.PostLikes
                    .Where(l => softDeletedUserIds.Contains(l.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.PostComments
                    .Where(c => softDeletedUserIds.Contains(c.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.EventEnrollments
                    .Where(e => softDeletedUserIds.Contains(e.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.PaymentRecords
                    .Where(p => softDeletedUserIds.Contains(p.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                // UserRoles is present on IApplicationDbContext
                await context.UserRoles
                    .Where(ur => softDeletedUserIds.Contains(ur.UserId))
                    .ExecuteDeleteAsync(cancellationToken);

                // Delete Identity claims/logins/tokens via Set<T>() without needing them on IApplicationDbContext
                if (dbContext != null)
                {
                    await dbContext.Set<IdentityUserClaim<Guid>>()
                        .Where(uc => softDeletedUserIds.Contains(uc.UserId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.Set<IdentityUserLogin<Guid>>()
                        .Where(ul => softDeletedUserIds.Contains(ul.UserId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.Set<IdentityUserToken<Guid>>()
                        .Where(ut => softDeletedUserIds.Contains(ut.UserId))
                        .ExecuteDeleteAsync(cancellationToken);
                }

                // Delete content where DeleteBehavior.Restrict is configured
                await context.Posts
                    .Where(p => softDeletedUserIds.Contains(p.AuthorId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.Events
                    .Where(e => softDeletedUserIds.Contains(e.CreatorId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.UploadedFiles
                    .Where(f => softDeletedUserIds.Contains(f.UploadedById))
                    .ExecuteDeleteAsync(cancellationToken);

                // 3. Permanently hard-delete the users (bypassing the soft-delete interceptor)
                var count = await context.Users
                    .IgnoreQueryFilters()
                    .Where(u => softDeletedUserIds.Contains(u.Id))
                    .ExecuteDeleteAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return count;
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
        }
    }
}