using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; set; }

    // FK (Customer domain id)
    public Guid CustomerId { get; set; }

    // Uygulama içi hesap numarasý (IBAN deðil) - opsiyonel
    public string? AccountNumber { get; set; }

    // ISO-4217 (TRY, USD ...)
    public string Currency { get; set; } = default!;

    public decimal Balance { get; set; }

    // DB tarafýnda int map edilecek (AccountConfiguration HasConversion<int>())
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    // Opsiyonel (dýþ dünya/entegrasyon için)
    public string? Iban { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<AccountingTransaction> Transactions { get; set; } = new List<AccountingTransaction>();
    public ICollection<AccountBalanceHistory> BalanceHistory { get; set; } = new List<AccountBalanceHistory>();
}
