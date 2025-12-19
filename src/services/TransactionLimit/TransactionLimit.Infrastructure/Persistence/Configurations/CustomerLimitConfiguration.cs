using GaniPay.TransactionLimit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.TransactionLimit.Infrastructure.Persistence.Configurations;

public sealed class CustomerLimitConfiguration : IEntityTypeConfiguration<CustomerLimit>
{
    public void Configure(EntityTypeBuilder<CustomerLimit> builder)
    {
        builder.ToTable("limit_customer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");

        builder.Property(x => x.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(x => x.LimitDefinitionId).HasColumnName("LimitDefinitionId").IsRequired();

        builder.Property(x => x.Year).HasColumnName("Year").IsRequired();
        builder.Property(x => x.Month).HasColumnName("Month");
        builder.Property(x => x.Day).HasColumnName("Day");

        builder.Property(x => x.Value).HasColumnName("Value").HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("Source")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Reason).HasColumnName("Reason").HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
        builder.Property(x => x.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(200);

        builder.HasOne(x => x.LimitDefinition)
            .WithMany()
            .HasForeignKey(x => x.LimitDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CustomerId, x.LimitDefinitionId, x.Year, x.Month, x.Day }).IsUnique();
    }
}
