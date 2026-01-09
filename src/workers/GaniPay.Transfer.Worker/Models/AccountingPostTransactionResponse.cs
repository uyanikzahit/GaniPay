using System;
using System.Text.Json.Serialization;

namespace GaniPay.Transfer.Worker.Models;

public sealed class AccountingPostTransactionResponse
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("accountId")] public string? AccountId { get; set; }
    [JsonPropertyName("direction")] public string? Direction { get; set; } // "debit"/"credit"
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }

    [JsonPropertyName("balanceBefore")] public decimal? BalanceBefore { get; set; }
    [JsonPropertyName("balanceAfter")] public decimal? BalanceAfter { get; set; }

    [JsonPropertyName("operationType")] public int? OperationType { get; set; }
    [JsonPropertyName("referenceId")] public string? ReferenceId { get; set; }
    [JsonPropertyName("createdAt")] public DateTimeOffset? CreatedAt { get; set; }
}
