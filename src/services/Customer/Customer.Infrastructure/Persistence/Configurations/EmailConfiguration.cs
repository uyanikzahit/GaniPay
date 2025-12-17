using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Customer.Infrastructure.Persistence.Configurations;

public sealed class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> b)
    {
        b.ToTable("email");
        b.HasKey(x => x.Id);

        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.EmailAddress)
            .HasColumnName("email_address")
            .HasMaxLength(320)
            .IsRequired();

        b.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.IsVerified)
            .HasColumnName("is_verified")
            .IsRequired();

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        b.HasIndex(x => new { x.CustomerId, x.EmailAddress }).IsUnique();
    }
}
