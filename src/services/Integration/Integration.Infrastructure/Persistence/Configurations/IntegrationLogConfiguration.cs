using GaniPay.Integration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Integration.Infrastructure.Persistence.Configurations;

public sealed class IntegrationLogConfiguration : IEntityTypeConfiguration<IntegrationLog>
{
    public void Configure(EntityTypeBuilder<IntegrationLog> b)
    {
        b.ToTable("integration_log");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");

        b.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        b.Property(x => x.Operation)
            .HasColumnName("operation")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.RequestPayload)
            .HasColumnName("request_payload")
            .IsRequired();

        b.Property(x => x.ResponsePayload)
            .HasColumnName("response_payload")
            .IsRequired();

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.HasIndex(x => new { x.ProviderId, x.CreatedAt });
        b.HasIndex(x => x.Operation);

        b.HasOne<IntegrationProvider>()
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
