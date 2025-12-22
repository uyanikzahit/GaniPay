namespace GaniPay.Accounting.Domain.Entities;

public sealed class AccountBalanceHistory
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;

    /// <summary>
    /// DB: direction (string) => "debit" / "credit"
    /// </summary>
    public string Direction { get; set; } = default!;

    /// <summary>
    /// DB: change_amount
    /// </summary>
    public decimal ChangeAmount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string Currency { get; set; } = default!;

    public int OperationType { get; set; }

    public Guid ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
