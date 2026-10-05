namespace Cessa.Application.Common.Interfaces
{
    using Cessa.Domain.Entities.Admin;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Domain.Entities.Content;
    using Cessa.Domain.Entities.Payment;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<ApplicationRole> Roles { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }
        DbSet<OtpRecord> OtpRecords { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Post> Posts { get; }
        DbSet<Event> Events { get; }
        DbSet<PostLike> PostLikes { get; }
        DbSet<PostComment> PostComments { get; }
        DbSet<EventEnrollment> EventEnrollments { get; }
        DbSet<HallOfFameMember> HallOfFameMembers { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<PaymentRecord> PaymentRecords { get; }
        DbSet<UploadedFile> UploadedFiles { get; }
        DbSet<Advertisement> Advertisements { get; }
        DbSet<PostCategory> PostCategories { get; }
        DbSet<UserCategoryPreference> UserCategoryPreferences { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
