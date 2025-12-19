namespace GaniPay.Expense.Application.Contracts.Dtos;

public sealed class ExpensePendingDto
{
    public Guid Id { get; init; }

    public Guid AccountingTxId { get; init; }
    public Guid ExpenseId { get; init; }

    public decimal CalculatedAmount { get; init; }
    public string Currency { get; init; } = default!;

    public string PendingStatus { get; init; } = default!;
    public DateTime TransactionDate { get; init; }

    public short TryCount { get; init; }
    public string? ResultCode { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
