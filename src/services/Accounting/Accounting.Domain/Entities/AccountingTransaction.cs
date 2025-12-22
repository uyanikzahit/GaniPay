namespace GaniPay.Accounting.Domain.Entities;

public sealed class AccountingTransaction
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;

    /// <summary>
    /// DB: direction (string) => "debit" / "credit"
    /// </summary>
    public string Direction { get; set; } = default!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = default!;

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// DB: operation_type (int)
    /// </summary>
    public int OperationType { get; set; }

    /// <summary>
    /// DB: reference_id (guid) - Payments/topup/transfer kaydý gibi korelasyon.
    /// </summary>
    public Guid ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
