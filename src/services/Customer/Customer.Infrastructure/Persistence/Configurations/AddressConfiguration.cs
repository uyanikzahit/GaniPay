using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Customer.Infrastructure.Persistence.Configurations;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.ToTable("address");
        b.HasKey(x => x.Id);

        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.AddressType)
            .HasColumnName("address_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        b.Property(x => x.District).HasColumnName("district").HasMaxLength(100).IsRequired();
        b.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20).IsRequired();
        b.Property(x => x.AddressLine1).HasColumnName("address_line_1").HasMaxLength(500).IsRequired();

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        b.HasIndex(x => x.CustomerId);
    }
}
