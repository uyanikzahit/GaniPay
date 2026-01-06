using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using GaniPay.LimitsControl.Worker.Models;

namespace GaniPay.LimitsControl.Worker.Handlers;

public sealed class WalletLimitDetailsGetJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        var vars = ParseVars(job.Variables);
        var currency = GetString(vars, "currency") ?? "TRY";

        var defs = currency switch
        {
            "TRY" => new LimitDefinitions { SingleMax = 5000, DailyMax = 20000 },
            "USD" => new LimitDefinitions { SingleMax = 200, DailyMax = 1000 },
            _ => new LimitDefinitions { SingleMax = 1000, DailyMax = 5000 }
        };

        var output = new
        {
            limitDefinitionsOk = true,
            limitDefinitions = new { singleMax = defs.SingleMax, dailyMax = defs.DailyMax }
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

    private static string? GetString(Dictionary<string, object?> v, string key)
    {
        if (!v.TryGetValue(key, out var val) || val is null) return null;
        if (val is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                _ => je.GetRawText()
            };
        }
        return val.ToString();
    }
}
