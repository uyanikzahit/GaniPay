using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class AccountStatusJobHandler
{
    private readonly HttpClient _http;

    public AccountStatusJobHandler(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Accounting");
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(job.Variables);
            var root = doc.RootElement;

            var accountId = root.TryGetProperty("accountId", out var a) ? a.GetString() : null;
            var customerId = root.TryGetProperty("customerId", out var c) ? c.GetString() : null;
            var currency = root.TryGetProperty("currency", out var cu) ? cu.GetString() : null;

            if (string.IsNullOrWhiteSpace(accountId))
            {
                await CompleteFail(client, job,
                    "ACCOUNT_ID_REQUIRED",
                    "accountId zorunludur.",
                    "Account Status Check");
                return;
            }

            // GET /api/accounting/accounts/status?accountId=...&customerId=...&currency=...
            var url =
                $"/api/accounting/accounts/status?accountId={Uri.EscapeDataString(accountId)}" +
                $"&customerId={Uri.EscapeDataString(customerId ?? string.Empty)}" +
                $"&currency={Uri.EscapeDataString(currency ?? string.Empty)}";

            var resp = await _http.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    "ACCOUNT_STATUS_HTTP_ERROR",
                    $"Accounting accounts/status HTTP {(int)resp.StatusCode}",
                    "Account Status Check");
                return;
            }

            // Eğer response body farklıysa bile success => ok kabul ediyoruz
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            bool accountOk = true;

            if (body.ValueKind == JsonValueKind.Object &&
                body.TryGetProperty("accountOk", out var okProp) &&
                (okProp.ValueKind == JsonValueKind.True || okProp.ValueKind == JsonValueKind.False))
            {
                accountOk = okProp.GetBoolean();
            }

            string status =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("status", out var st) ? (st.GetString() ?? "Active") : "Active";

            string statusText =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("accountStatusText", out var tx) ? (tx.GetString() ?? "OK") : "OK";

            var completeVars = new
            {
                accountOk = accountOk,
                status = status,                 // BPMN output -> accountStatus
                accountStatusText = statusText,
                errorCode = (string?)null,
                errorMessage = (string?)null,
                failedAtStep = (string?)null
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars))
                .Send();
        }
        catch (Exception ex)
        {
            await CompleteFail(client, job,
                "ACCOUNT_STATUS_EXCEPTION",
                ex.Message,
                "Account Status Check");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            accountOk = false,
            status = "Failed",
            accountStatusText = "FAILED",
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}
