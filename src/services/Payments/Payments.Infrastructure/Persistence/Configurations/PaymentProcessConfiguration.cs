using GaniPay.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentProcessConfiguration : IEntityTypeConfiguration<PaymentProcess>
{
    public void Configure(EntityTypeBuilder<PaymentProcess> b)
    {
        // TABLO
        b.ToTable("payment_process");

        // PK
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");

        // Correlation + Idempotency
        b.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100)
            .IsRequired();

        // Customer
        b.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        // Enums (short) -> smallint
        b.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<short>()
            .IsRequired();

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        // Amount / Currency
        b.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        // Workflow
        b.Property(x => x.WorkflowInstanceKey)
            .HasColumnName("workflow_instance_key");

        // Error
        b.Property(x => x.ErrorCode)
            .HasColumnName("error_code")
            .HasMaxLength(100);

        b.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        // Audit
        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        b.HasIndex(x => x.CorrelationId).IsUnique();
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.HasIndex(x => new { x.CustomerId, x.CreatedAt });
    }
}
