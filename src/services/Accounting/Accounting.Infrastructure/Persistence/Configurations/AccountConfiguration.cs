using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainAccount = GaniPay.Accounting.Domain.Entities.Account;

namespace GaniPay.Accounting.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<DomainAccount>
{
    public void Configure(EntityTypeBuilder<DomainAccount> builder)
    {
        builder.ToTable("account");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(64);

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasColumnName("balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        // DB’de status integer ise enum’u int olarak map et
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Iban)
            .HasColumnName("iban")
            .HasMaxLength(64);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => new { x.CustomerId, x.Currency }).IsUnique();
    }
}
