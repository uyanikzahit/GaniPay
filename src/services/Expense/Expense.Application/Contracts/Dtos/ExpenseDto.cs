namespace GaniPay.Expense.Application.Contracts.Dtos;

public sealed class ExpenseDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public decimal? Percent { get; init; }
    public decimal? FixedAmount { get; init; }

    public string Currency { get; init; } = default!;
    public bool IsVisible { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
