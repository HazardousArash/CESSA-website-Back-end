namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
    {
        public void Configure(EntityTypeBuilder<UploadedFile> builder)
        {
            builder.HasKey(f => f.Id);
            builder.HasQueryFilter(f => !f.IsDeleted);

            builder.HasOne(f => f.UploadedBy)
                   .WithMany()
                   .HasForeignKey(f => f.UploadedById)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}