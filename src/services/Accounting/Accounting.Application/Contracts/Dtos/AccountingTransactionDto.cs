using GaniPay.Accounting.Application.Contracts.Enums;
using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed class AccountingTransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    public AccountingEntryDirection Direction { get; set; }
    public AccountingOperationType OperationType { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string ReferenceId { get; set; } = default!;
    public string IdempotencyKey { get; set; } = default!;
    public string CorrelationId { get; set; } = default!;

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? BookedAt { get; set; }
}
