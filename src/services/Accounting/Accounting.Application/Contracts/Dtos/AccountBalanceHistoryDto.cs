using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed class AccountBalanceHistoryDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    public AccountingEntryDirection Direction { get; set; }
    public decimal ChangeAmount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string Currency { get; set; } = default!;
    public AccountingOperationType OperationType { get; set; }
    public string ReferenceId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
