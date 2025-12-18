using GaniPay.TransactionLimit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.TransactionLimit.Infrastructure.Persistence.Configurations;

public sealed class LimitDefinitionConfiguration : IEntityTypeConfiguration<LimitDefinition>
{
    public void Configure(EntityTypeBuilder<LimitDefinition> builder)
    {
        builder.ToTable("limit_definition");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.Period)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.MetricType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsVisible)
            .IsRequired();
    }
}
