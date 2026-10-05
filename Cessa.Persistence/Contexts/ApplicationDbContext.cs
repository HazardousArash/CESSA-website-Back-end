namespace Cessa.Persistence.Contexts
{
    using Cessa.Application.Common.Interfaces;
    using Cessa.Domain.Entities.Admin;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Domain.Entities.Content;
    using Cessa.Domain.Entities.Payment;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.Reflection;
    using System;

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
    {
        public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<PostComment> PostComments => Set<PostComment>();
        public DbSet<EventEnrollment> EventEnrollments => Set<EventEnrollment>();
        public DbSet<HallOfFameMember> HallOfFameMembers => Set<HallOfFameMember>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
        public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
        public DbSet<Advertisement> Advertisements => Set<Advertisement>();
        public DbSet<PostCategory> PostCategories => Set<PostCategory>();
        public DbSet<UserCategoryPreference> UserCategoryPreferences => Set<UserCategoryPreference>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
