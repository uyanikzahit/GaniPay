using System.Text.Json.Serialization;

namespace GaniPay.TopUp.Worker.Models;

public sealed class TopUpVariables
{
    // Inputs (BPMN)
    [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
    [JsonPropertyName("accountId")] public string? AccountId { get; set; }
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("idempotencyKey")] public string? IdempotencyKey { get; set; }

    // Akış başlangıcında varsa
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }

    // Validate / Initiate input mapping’inde kullandığın isim
    [JsonPropertyName("workflowCorrelationId")] public string? WorkflowCorrelationId { get; set; }

    // Initiate output ile set edeceğiz
    [JsonPropertyName("paymentCorrelationId")] public string? PaymentCorrelationId { get; set; }

    // Akış boyunca durumlar
    [JsonPropertyName("paymentStatus")] public string? PaymentStatus { get; set; }
    [JsonPropertyName("accountStatus")] public string? AccountStatus { get; set; }

    // Common error fields
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
    [JsonPropertyName("failedAtStep")] public string? FailedAtStep { get; set; }
}
