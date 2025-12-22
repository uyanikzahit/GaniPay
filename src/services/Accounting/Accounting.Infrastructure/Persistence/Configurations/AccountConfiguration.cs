using GaniPay.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Accounting.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("account");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");

        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.AccountNumber).HasColumnName("account_number").HasMaxLength(100).IsRequired();

        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();

        b.Property(x => x.Balance).HasColumnName("balance").HasPrecision(18, 2).IsRequired();

        b.Property(x => x.Status).HasColumnName("status").IsRequired();

        b.Property(x => x.Iban).HasColumnName("iban").HasMaxLength(64);

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        b.HasIndex(x => new { x.CustomerId, x.Currency }).IsUnique();

        // relations
        b.HasMany(x => x.Transactions)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId);

        b.HasMany(x => x.BalanceHistories)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId);
    }
}
