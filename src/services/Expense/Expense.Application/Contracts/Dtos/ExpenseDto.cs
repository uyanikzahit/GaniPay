namespace GaniPay.Expense.Application.Contracts.Dtos;

public sealed class ExpenseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? Percent { get; set; }
    public decimal? FixedAmount { get; set; }

    public string Currency { get; set; } = "TRY";
    public bool IsVisible { get; set; }
}
