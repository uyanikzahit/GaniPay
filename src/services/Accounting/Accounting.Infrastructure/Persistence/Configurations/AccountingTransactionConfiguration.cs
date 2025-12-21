using GaniPay.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Accounting.Infrastructure.Persistence.Configurations;

public sealed class AccountingTransactionConfiguration : IEntityTypeConfiguration<AccountingTransaction>
{
    public void Configure(EntityTypeBuilder<AccountingTransaction> builder)
    {
        builder.ToTable("accounting_transaction");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(x => x.Direction)
            .HasColumnName("direction")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.OperationType)
            .HasColumnName("operation_type")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(x => x.ReferenceId).HasColumnName("reference_id").HasMaxLength(150).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(150).IsRequired();
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(150).IsRequired();


        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.BookedAt).HasColumnName("booked_at").HasColumnType("timestamptz");

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("ux_accounting_transaction_idempotency");

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("ix_accounting_transaction_account_id");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
