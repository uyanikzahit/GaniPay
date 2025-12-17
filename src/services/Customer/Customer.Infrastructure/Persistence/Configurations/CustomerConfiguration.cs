using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CustomerEntity = GaniPay.Customer.Domain.Entities.Customer;

namespace GaniPay.Customer.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        // DB'de tablo adý "customer"
        builder.ToTable("customer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerNumber)
            .HasColumnName("customer_number")
            .IsRequired();

        // Enum'larý DB'de string tutuyorsan:
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Segment)
            .HasColumnName("segment")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.OpenDate)
            .HasColumnName("open_date")
            .IsRequired();

        builder.Property(x => x.CloseDate)
            .HasColumnName("close_date");

        builder.Property(x => x.CloseReason)
            .HasColumnName("close_reason");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
