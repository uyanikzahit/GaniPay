using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed class AccountDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
