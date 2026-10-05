namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Admin;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HallOfFameMemberConfiguration : IEntityTypeConfiguration<HallOfFameMember>
    {
        public void Configure(EntityTypeBuilder<HallOfFameMember> builder)
        {
            builder.HasKey(h => h.Id);

            // This is the crucial line that hides deleted members from all queries
            builder.HasQueryFilter(h => !h.IsDeleted);

            builder.HasOne(h => h.PhotoFile)
                   .WithMany()
                   .HasForeignKey(h => h.PhotoFileId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}