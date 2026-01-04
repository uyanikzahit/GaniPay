namespace GaniPay.TopUp.Worker.Models;

public sealed class AccountingPostTransactionResponse
{
    public string Id { get; set; } = default!;
    public Guid AccountId { get; set; }

    // API’nin döndürdüğü örneğe göre string de gelebilir, ikisini de tolere etmek için string tuttuk
    public string Direction { get; set; } = default!; // "credit" gibi
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public int OperationType { get; set; }
    public string ReferenceId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}