using GaniPay.Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Expense.Infrastructure.Persistence.Configurations;

public sealed class ExpensePendingConfiguration : IEntityTypeConfiguration<ExpensePending>
{
    public void Configure(EntityTypeBuilder<ExpensePending> builder)
    {
        builder.ToTable("expense_pending");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.AccountingTxId).HasColumnName("accounting_tx_id").IsRequired();
        builder.Property(x => x.ExpenseId).HasColumnName("expense_id").IsRequired();

        builder.Property(x => x.CalculatedAmount).HasColumnName("calculated_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasColumnName("currency").IsRequired();

        builder.Property(x => x.PendingStatus).HasColumnName("pending_status").HasConversion<string>().IsRequired();
        builder.Property(x => x.TransactionDate).HasColumnName("transaction_date").IsRequired();

        builder.Property(x => x.TryCount).HasColumnName("try_count");
        builder.Property(x => x.ResultCode).HasColumnName("result_code");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Expense)
            .WithMany()
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
