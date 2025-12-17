using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Customer.Infrastructure.Persistence.Configurations;

public sealed class CustomerIndividualConfiguration : IEntityTypeConfiguration<CustomerIndividual>
{
    public void Configure(EntityTypeBuilder<CustomerIndividual> b)
    {
        b.ToTable("customer_individual");
        b.HasKey(x => x.Id);

        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();

        b.Property(x => x.BirthDate)
            .HasColumnName("birth_date")
            .HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v))
            .IsRequired();

        b.Property(x => x.Nationality).HasColumnName("nationality").HasMaxLength(100).IsRequired();
        b.Property(x => x.IdentityNumber).HasColumnName("identity_number").HasMaxLength(20).IsRequired();

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        b.HasIndex(x => x.IdentityNumber).IsUnique();
        b.HasIndex(x => x.CustomerId).IsUnique();
    }
}
