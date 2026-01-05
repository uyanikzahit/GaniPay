using System.Text.Json.Serialization;

namespace GaniPay.TopUp.Worker.Models;

public sealed class AccountingAccountStatusResponse
{
    [JsonPropertyName("accountOk")] public bool AccountOk { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("accountStatusText")] public string? AccountStatusText { get; set; }
}
