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

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.LimitDefinitionId).IsRequired();

        builder.Property(x => x.Value)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(255);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        builder.HasOne(x => x.LimitDefinition)
            .WithMany()
            .HasForeignKey(x => x.LimitDefinitionId);
    }
}
