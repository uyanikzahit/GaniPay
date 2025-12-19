namespace GaniPay.Expense.Application.Requests;

public sealed class CreateExpenseRequest
{
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public decimal? Percent { get; init; }
    public decimal? FixedAmount { get; init; }

    public string Currency { get; init; } = "TRY";
    public bool IsVisible { get; init; } = true;
}
