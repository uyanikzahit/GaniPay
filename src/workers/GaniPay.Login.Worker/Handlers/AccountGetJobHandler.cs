using GaniPay.Login.Worker.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class AccountGetJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AccountingApiOptions _opt;

    public AccountGetJobHandler(IHttpClientFactory httpClientFactory, IOptions<AccountingApiOptions> opt)
    {
        _httpClientFactory = httpClientFactory;
        _opt = opt.Value;
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        using var doc = JsonDocument.Parse(job.Variables);
        var root = doc.RootElement;

        var customerId = TryGetString(root, "customerId");
        var accessToken = TryGetString(root, "accessToken");

        if (string.IsNullOrWhiteSpace(customerId))
        {
            await CompleteAsync(client, job, new
            {
                accountOk = false,
                errorCode = "ACCOUNT_INVALID_INPUT",
                wallets = (object?)null
            });
            return;
        }

        try
        {
            var baseUrl = (_opt.BaseUrl ?? "http://host.docker.internal:5103").TrimEnd('/');
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(baseUrl);
            http.Timeout = TimeSpan.FromSeconds(15);

            if (!string.IsNullOrWhiteSpace(accessToken))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/accounting/customers/{customerId}/wallets")
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };

            var resp = await http.SendAsync(req);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                await CompleteAsync(client, job, new
                {
                    accountOk = false,
                    errorCode = "WALLETS_NOT_FOUND",
                    wallets = (object?)null
                });
                return;
            }

            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteAsync(client, job, new
                {
                    accountOk = false,
                    errorCode = $"ACCOUNTING_HTTP_{(int)resp.StatusCode}",
                    wallets = (object?)null,
                    accountingError = raw
                });
                return;
            }

            using var body = JsonDocument.Parse(raw);
            var b = body.RootElement;

            var respCustomerId = GetString(b, "customerId") ?? customerId;

            // accounts array -> minimal liste
            var accounts = GetAccountsMinimal(b);

            // ✅ accountOk: accounts var ve en az 1 tane var ise true
            var ok = accounts is not null && accounts.Count > 0;

            var walletsOut = new
            {
                customerId = respCustomerId,
                accounts // [{accountId,currency,balance,status}]
            };

            await CompleteAsync(client, job, new
            {
                accountOk = ok,
                errorCode = ok ? null : "NO_WALLET_ACCOUNT",
                wallets = walletsOut
            });
        }
        catch (Exception ex)
        {
            await CompleteAsync(client, job, new
            {
                accountOk = false,
                errorCode = "ACCOUNTING_EXCEPTION",
                wallets = (object?)null,
                accountingError = ex.Message
            });
        }
    }

    private static List<object>? GetAccountsMinimal(JsonElement root)
    {
        if (!root.TryGetProperty("accounts", out var arr)) return null;
        if (arr.ValueKind != JsonValueKind.Array) return null;

        var list = new List<object>();
        foreach (var a in arr.EnumerateArray())
        {
            if (a.ValueKind != JsonValueKind.Object) continue;

            list.Add(new
            {
                accountId = GetString(a, "id"),
                currency = GetString(a, "currency"),
                balance = GetDecimal(a, "balance"),
                status = GetInt(a, "status")
            });
        }
        return list;
    }

    private static async Task CompleteAsync(IJobClient client, IJob job, object variables)
    {
        var json = JsonSerializer.Serialize(variables, JsonOpts);
        await client.NewCompleteJobCommand(job.Key).Variables(json).Send();
    }

    private static string? TryGetString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;
        return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
    }

    private static string? GetString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;
        if (p.ValueKind == JsonValueKind.String) return p.GetString();
        if (p.ValueKind == JsonValueKind.Number) return p.GetRawText();
        return null;
    }

    private static decimal GetDecimal(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return 0m;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
        if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var ds)) return ds;
        return 0m;
    }

    private static int GetInt(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return 0;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
        if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var isx)) return isx;
        return 0;
    }
}
