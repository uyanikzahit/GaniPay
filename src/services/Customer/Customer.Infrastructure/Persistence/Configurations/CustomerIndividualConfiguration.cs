using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Customer.Infrasturcture.Persistence.Configurations;

public sealed class CustomerIndividualConfiguration : IEntityTypeConfiguration<CustomerIndividual>
{
    public void Configure(EntityTypeBuilder<CustomerIndividual> builder)
    {
        builder.ToTable("customer_individual");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.HasIndex(x => x.CustomerId)
            .IsUnique();

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(x => x.Nationality)
            .HasColumnName("nationality")
            .HasMaxLength(100);

        builder.Property(x => x.IdentityNumber)
            .HasColumnName("identity_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.IdentityNumber)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
