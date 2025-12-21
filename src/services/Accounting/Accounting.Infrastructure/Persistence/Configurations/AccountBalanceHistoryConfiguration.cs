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

        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.Direction).IsRequired();
        builder.Property(x => x.ChangeAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.BalanceBefore).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.OperationType).IsRequired();

        builder.Property(x => x.ReferenceId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Account)
               .WithMany()
               .HasForeignKey(x => x.AccountId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
