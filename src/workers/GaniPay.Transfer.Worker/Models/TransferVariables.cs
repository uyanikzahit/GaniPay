using System.Text.Json.Serialization;

namespace GaniPay.Transfer.Worker.Models;

public sealed class TransferVariables
{
    // BPMN input: customerId
    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    // BPMN input: receiverCustomerId
    [JsonPropertyName("receiverCustomerId")]
    public string? ReceiverCustomerId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    // sende bazen geliyor bazen gelmiyor -> default TRY kullanacaðýz
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }
}
