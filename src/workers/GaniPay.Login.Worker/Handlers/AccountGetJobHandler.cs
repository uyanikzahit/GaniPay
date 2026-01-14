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

            if (!resp.IsSuccessStatusCode)
            {
                var raw = await resp.Content.ReadAsStringAsync();
                await CompleteAsync(client, job, new
                {
                    accountOk = false,
                    errorCode = $"ACCOUNTING_HTTP_{(int)resp.StatusCode}",
                    wallets = (object?)null,
                    accountingError = raw
                });
                return;
            }

            using var bodyDoc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var wallets = bodyDoc.RootElement.Clone();

            await CompleteAsync(client, job, new
            {
                accountOk = true,
                errorCode = (string?)null,
                wallets
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
}
