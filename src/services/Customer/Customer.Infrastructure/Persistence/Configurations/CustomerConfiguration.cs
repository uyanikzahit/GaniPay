using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CustomerEntity = GaniPay.Customer.Domain.Entities.Customer;

namespace GaniPay.Customer.Infrasturcture.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("customer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerNumber)
            .HasColumnName("customer_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.CustomerNumber)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Segment)
            .HasColumnName("segment")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OpenDate)
            .HasColumnName("open_date")
            .IsRequired();

        builder.Property(x => x.CloseDate)
            .HasColumnName("close_date");

        builder.Property(x => x.CloseReason)
            .HasColumnName("close_reason")
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // 1-1: Customer -> CustomerIndividual
        builder.HasOne(x => x.Individual)
            .WithOne(x => x.Customer)
            .HasForeignKey<GaniPay.Customer.Domain.Entities.CustomerIndividual>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-N: Customer -> Emails/Phones/Addresses
        builder.HasMany(x => x.Emails)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Phones)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Addresses)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
