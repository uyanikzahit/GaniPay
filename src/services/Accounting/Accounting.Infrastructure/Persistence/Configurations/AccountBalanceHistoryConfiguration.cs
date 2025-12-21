using GaniPay.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Accounting.Infrastructure.Persistence.Configurations;

public sealed class AccountBalanceHistoryConfiguration : IEntityTypeConfiguration<AccountBalanceHistory>
{
    public void Configure(EntityTypeBuilder<AccountBalanceHistory> builder)
    {
        builder.ToTable("account_balance_history");

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

        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.ChangeAmount).HasColumnName("change_amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(x => x.ReferenceId).HasColumnName("reference_id").HasMaxLength(150).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("ix_account_balance_history_account_id");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
