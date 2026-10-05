namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventEnrollmentConfiguration : IEntityTypeConfiguration<EventEnrollment>
    {
        public void Configure(EntityTypeBuilder<EventEnrollment> builder)
        {
            builder.HasKey(e => e.Id);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Break cascade path from Event -> Enrollment
            builder.HasOne(e => e.Event)
                   .WithMany(ev => ev.Enrollments)
                   .HasForeignKey(e => e.EventId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Break cascade path from User -> Enrollment
            builder.HasOne(e => e.User)
                   .WithMany()
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}