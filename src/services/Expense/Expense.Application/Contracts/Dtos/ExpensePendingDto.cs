namespace GaniPay.Expense.Application.Contracts.Dtos;

public sealed class ExpensePendingDto
{
    public Guid Id { get; set; }
    public Guid AccountingTxId { get; set; }
    public Guid ExpenseId { get; set; }

    public decimal CalculatedAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    public string PendingStatus { get; set; } = "Pending";
    public DateTime TransactionDate { get; set; }

    public short TryCount { get; set; }
    public string? ResultCode { get; set; }
}
