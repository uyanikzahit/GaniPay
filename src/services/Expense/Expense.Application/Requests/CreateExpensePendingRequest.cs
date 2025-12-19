namespace GaniPay.Expense.Application.Requests;

public sealed class CalculateExpenseRequest
{
    public string Code { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "TRY";
}
