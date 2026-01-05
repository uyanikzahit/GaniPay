using System.Text.Json.Serialization;

namespace GaniPay.TopUp.Worker.Models;

public sealed class AccountingPostTransactionResponse
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("balanceBefore")] public decimal? BalanceBefore { get; set; }
    [JsonPropertyName("balanceAfter")] public decimal? BalanceAfter { get; set; }
    [JsonPropertyName("createdAt")] public DateTimeOffset? CreatedAt { get; set; }
    [JsonPropertyName("referenceId")] public string? ReferenceId { get; set; }
    [JsonPropertyName("operationType")] public int? OperationType { get; set; }
}
