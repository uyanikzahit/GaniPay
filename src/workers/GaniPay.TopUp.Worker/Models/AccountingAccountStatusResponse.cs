namespace GaniPay.TopUp.Worker.Models;

public sealed class AccountingAccountStatusResponse
{
    public Guid AccountId { get; set; }
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = default!;
    public short Status { get; set; } // Active=1 Passive=2 Blocked=3
}