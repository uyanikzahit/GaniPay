using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.LimitsControl.Worker.Handlers;

public sealed class AccountsGetJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        var vars = ParseVars(job.Variables);
        var accountId = GetString(vars, "accountId");

        var ok = !string.IsNullOrWhiteSpace(accountId);

        var output = new
        {
            accountsOk = ok,
            accountStatus = ok ? "ACTIVE" : "NOT_FOUND"
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
