namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(e => e.Id);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Break cascade path from User -> Event
            builder.HasOne(e => e.Creator)
                   .WithMany()
                   .HasForeignKey(e => e.CreatorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}