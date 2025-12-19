using GaniPay.Expense.Domain.Enums;

namespace GaniPay.Expense.Domain.Entities;

public sealed class ExpensePending : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid AccountingTxId { get; set; }
    public Guid ExpenseId { get; set; }

    public decimal CalculatedAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    public ExpensePendingStatus PendingStatus { get; set; } = ExpensePendingStatus.Pending;
    public DateTime TransactionDate { get; set; }

    public short TryCount { get; set; }
    public string? ResultCode { get; set; }

    // opsiyonel navigation
    public ExpenseDefinition? Expense { get; set; }
}
