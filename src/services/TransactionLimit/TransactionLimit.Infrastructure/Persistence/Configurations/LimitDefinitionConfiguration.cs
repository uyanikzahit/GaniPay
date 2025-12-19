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

        builder.Property(x => x.Id)
            .HasColumnName("Id");

        builder.Property(x => x.Code)
            .HasColumnName("Code")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.Property(x => x.Period)
            .HasColumnName("Period")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.MetricType)
            .HasColumnName("MetricType")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.IsVisible)
            .HasColumnName("IsVisible")
            .IsRequired();
    }
}
