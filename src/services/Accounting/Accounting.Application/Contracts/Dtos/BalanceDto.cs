namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed class BalanceDto
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public DateTime AsOfUtc { get; set; }
}
