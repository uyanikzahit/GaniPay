using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Domain.Entities;

public sealed class AccountingTransaction
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;

    public EntryDirection Direction { get; set; }
    public OperationType OperationType { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    // Payments/Workflow tarafýndaki business referans (transfer-0001 gibi)
    public string ReferenceId { get; set; } = default!;

    // duplicate booking engellemek için
    public string IdempotencyKey { get; set; } = default!;

    // camunda/workflow correlation
    public string CorrelationId { get; set; } = default!;

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BookedAt { get; set; }
}
