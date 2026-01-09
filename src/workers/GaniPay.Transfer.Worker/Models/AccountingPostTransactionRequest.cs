using System.Text.Json.Serialization;

namespace GaniPay.Transfer.Worker.Models;

public sealed class AccountingPostTransactionRequest
{
    [JsonPropertyName("accountId")] public string? AccountId { get; set; }

    // swagger’da numeric kullanmýþsýn: 1=debit, 2=credit
    [JsonPropertyName("direction")] public int Direction { get; set; }

    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }

    // Transfer için senin operasyon tipin neyse onu kullanacaksýn (örn 3)
    [JsonPropertyName("operationType")] public int OperationType { get; set; }

    [JsonPropertyName("referenceId")] public string? ReferenceId { get; set; }
}
