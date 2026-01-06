using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.LimitsControl.Worker.Handlers;

public sealed class WalletToWalletLimitsControlJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        var vars = ParseVars(job.Variables);
        var amount = GetDecimal(vars, "amount") ?? 0m;

        // mock kural: 0 < amount <= 5000 OK
        var ok = amount > 0m && amount <= 5000m;

        var output = new
        {
            limitOk = ok,
            limitReason = ok ? "OK" : "LIMIT_EXCEEDED"
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(output, JsonOpts))
            .Send();
    }

    private static Dictionary<string, object?> ParseVars(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOpts) ?? new();
        }
        catch { return new(); }
    }

    private static decimal? GetDecimal(Dictionary<string, object?> v, string key)
    {
        if (!v.TryGetValue(key, out var val) || val is null) return null;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number && je.TryGetDecimal(out var d)) return d;
            if (je.ValueKind == JsonValueKind.String && decimal.TryParse(je.GetString(), out var d2)) return d2;
        }

        if (val is decimal dd) return dd;
        if (decimal.TryParse(val.ToString(), out var d3)) return d3;

        return null;
    }
}
