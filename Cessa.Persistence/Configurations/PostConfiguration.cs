namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Content).IsRequired();

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(p => !p.IsDeleted);

            // Relationships
            builder.HasOne(p => p.Author)
                   .WithMany()
                   .HasForeignKey(p => p.AuthorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}