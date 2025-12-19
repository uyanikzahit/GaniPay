namespace GaniPay.Expense.Domain.Entities;

public sealed class ExpenseDefinition : AuditableEntity
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public decimal? Percent { get; private set; }      // 0.02 = %2
    public decimal? FixedAmount { get; private set; }  // sabit ücret
    public string Currency { get; private set; } = "TRY";
    public bool IsVisible { get; private set; } = true;

    private ExpenseDefinition() { }

    public ExpenseDefinition(
        Guid id,
        string code,
        string name,
        string? description,
        decimal? minAmount,
        decimal? maxAmount,
        decimal? percent,
        decimal? fixedAmount,
        string currency,
        bool isVisible)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        SetCode(code);
        SetName(name);

        Description = description;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Percent = percent;
        FixedAmount = fixedAmount;

        Currency = NormalizeCurrency(currency);
        IsVisible = isVisible;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description,
        decimal? minAmount,
        decimal? maxAmount,
        decimal? percent,
        decimal? fixedAmount,
        string currency,
        bool isVisible)
    {
        SetName(name);
        Description = description;

        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Percent = percent;
        FixedAmount = fixedAmount;

        Currency = NormalizeCurrency(currency);
        IsVisible = isVisible;

        Touch();
    }

    public decimal CalculateFee(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Tutar negatif olamaz.");

        if (MinAmount.HasValue && amount < MinAmount.Value) return 0m;
        if (MaxAmount.HasValue && amount > MaxAmount.Value) return 0m;

        var fee = 0m;

        if (Percent.HasValue && Percent.Value > 0)
            fee += amount * Percent.Value;

        if (FixedAmount.HasValue && FixedAmount.Value > 0)
            fee += FixedAmount.Value;

        return decimal.Round(fee, 2, MidpointRounding.AwayFromZero);
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code boþ olamaz.", nameof(code));

        Code = code.Trim().ToUpperInvariant();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name boþ olamaz.", nameof(name));

        Name = name.Trim();
    }

    private static string NormalizeCurrency(string currency)
    {
        var c = (currency ?? "").Trim().ToUpperInvariant();
        return string.IsNullOrWhiteSpace(c) ? "TRY" : c;
    }
}
