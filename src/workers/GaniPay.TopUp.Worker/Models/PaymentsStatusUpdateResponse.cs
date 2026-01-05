using System.Text.Json.Serialization;

namespace GaniPay.TopUp.Worker.Models;

public sealed class PaymentsStatusUpdateResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
}
