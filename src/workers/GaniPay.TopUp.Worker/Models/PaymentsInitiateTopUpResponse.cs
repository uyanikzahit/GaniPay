using System.Text.Json.Serialization;

namespace GaniPay.TopUp.Worker.Models;

public sealed class PaymentsInitiateTopUpResponse
{
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
}
