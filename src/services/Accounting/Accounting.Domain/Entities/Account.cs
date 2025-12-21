using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
