namespace GaniPay.Expense.Application.Requests;

public sealed class CreateExpensePendingRequest
{
    public Guid AccountingTxId { get; set; }

    // Expense tanýmýný baðlamak için
    public Guid ExpenseId { get; set; }

    // Hesaplama için baz tutar (transfer/topup tutarý gibi)
    public decimal BaseAmount { get; set; }

    public string Currency { get; set; } = "TRY";
    public DateTime? TransactionDate { get; set; }
}
