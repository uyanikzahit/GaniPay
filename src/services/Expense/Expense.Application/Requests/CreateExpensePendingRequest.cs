namespace GaniPay.Expense.Application.Requests;

public sealed class CreateExpensePendingRequest
{
    // Accounting domain transaction id (cross-domain reference)
    public Guid AccountingTxId { get; set; }

    // ExpenseDefinition id (same DB)
    public Guid ExpenseId { get; set; }

    // Calculated fee amount (mandatory)
    public decimal CalculatedAmount { get; set; }

    // Optional (if empty, we will use ExpenseDefinition.Currency)
    public string? Currency { get; set; }
}
