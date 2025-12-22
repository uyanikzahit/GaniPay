using GaniPay.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Accounting.Infrastructure.Persistence.Configurations;

public sealed class AccountingTransactionConfiguration : IEntityTypeConfiguration<AccountingTransaction>
{
    public void Configure(EntityTypeBuilder<AccountingTransaction> b)
    {
        b.ToTable("accounting_transaction");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");

        b.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();

        b.Property(x => x.Direction).HasColumnName("direction").HasMaxLength(16).IsRequired();

        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();

        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();

        b.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasPrecision(18, 2).IsRequired();

        b.Property(x => x.OperationType).HasColumnName("operation_type").IsRequired();

        b.Property(x => x.ReferenceId).HasColumnName("reference_id").IsRequired();

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => new { x.Currency, x.CreatedAt });
    }
}
