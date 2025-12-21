using GaniPay.Accounting.Domain.Enums;

namespace GaniPay.Accounting.Domain.Entities;

public sealed class AccountBalanceHistory
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;

    public EntryDirection Direction { get; set; }
    public decimal ChangeAmount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string Currency { get; set; } = "TRY";
    public OperationType OperationType { get; set; }

    // ✅ string kalıyor. AccountingTransaction.Id’yi yazacaksan: tx.Id.ToString()
    public string ReferenceId { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
