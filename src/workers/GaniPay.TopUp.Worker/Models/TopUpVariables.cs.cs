namespace GaniPay.TopUp.Worker.Models;

public sealed class TopUpVariables
{
    public Guid CustomerId { get; set; }
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";

    public string? IdempotencyKey { get; set; }

    // Payments servisinden gelecek
    public string? CorrelationId { get; set; }

    // ortak hata alanları
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // step sonuçları (gateway’leri sürecek)
    public bool IsValid { get; set; }
    public bool AccountOk { get; set; }
    public bool LimitOk { get; set; }
    public bool OrderOk { get; set; }
    public bool ProviderOk { get; set; }
    public bool CreditOk { get; set; }
    public bool PersistOk { get; set; }
    public bool NotifyOk { get; set; }

    // Accounting çıktılarını saklamak istersen
    public string? AccountingTxId { get; set; }
    public decimal? BalanceAfter { get; set; }
}
