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
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
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

            // Swagger output: { accountId, customerId, currency, status: 1|2|3 }
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            // status int: 1=Active, 2=Passive, 3=Blocked
            var statusCode = 1; // default Active
            if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("status", out var st))
            {
                if (st.ValueKind == JsonValueKind.Number && st.TryGetInt32(out var s))
                    statusCode = s;
                else if (st.ValueKind == JsonValueKind.String && int.TryParse(st.GetString(), out var s2))
                    statusCode = s2;
            }

            var accountStatusText = statusCode switch
            {
                1 => "Active",
                2 => "Passive",
                3 => "Blocked",
                _ => "Unknown"
            };

            // iş kuralı: sadece Active ise OK
            var accountOk = statusCode == 1;

            var completeVars = new
            {
                accountOk = accountOk,
                accountStatus = statusCode,           // ✅ BPMN output ismi ile birebir
                accountStatusText = accountStatusText, // ✅
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
            accountStatus = -1,               // ✅ BPMN output ismi ile birebir
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
