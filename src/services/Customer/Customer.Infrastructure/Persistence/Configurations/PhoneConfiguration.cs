using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Customer.Infrastructure.Persistence.Configurations;

public sealed class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> b)
    {
        b.ToTable("phone");
        b.HasKey(x => x.Id);

        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(5)
            .IsRequired();

        b.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        b.HasIndex(x => new { x.CustomerId, x.CountryCode, x.PhoneNumber }).IsUnique();
    }
}
