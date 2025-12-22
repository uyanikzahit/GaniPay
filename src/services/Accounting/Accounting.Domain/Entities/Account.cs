using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string AccountNumber { get; set; } = default!;

    public string Currency { get; set; } = default!;

    public decimal Balance { get; set; }

    public AccountStatus Status { get; set; }

    public string? Iban { get; set; }

    public DateTime CreatedAt { get; set; }

    // navigation
    public ICollection<AccountingTransaction> Transactions { get; set; } = new List<AccountingTransaction>();
    public ICollection<AccountBalanceHistory> BalanceHistories { get; set; } = new List<AccountBalanceHistory>();
}
