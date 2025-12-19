namespace GaniPay.Expense.Application.Requests;

public sealed class CreateExpenseRequest
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? Percent { get; set; }      // 0.02 gibi
    public decimal? FixedAmount { get; set; }  // sabit masraf

    public string Currency { get; set; } = "TRY";
    public bool IsVisible { get; set; } = true;
}
