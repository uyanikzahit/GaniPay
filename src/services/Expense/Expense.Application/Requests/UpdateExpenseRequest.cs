namespace GaniPay.Expense.Application.Requests;

public sealed class UpdateExpenseRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? Percent { get; set; }
    public decimal? FixedAmount { get; set; }

    public string Currency { get; set; } = "TRY";
    public bool IsVisible { get; set; } = true;
}
