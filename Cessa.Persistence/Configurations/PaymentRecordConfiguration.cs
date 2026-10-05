namespace Cessa.Persistence.Configurations
{
    using Cessa.Domain.Entities.Payment;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount).HasPrecision(18, 2);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}