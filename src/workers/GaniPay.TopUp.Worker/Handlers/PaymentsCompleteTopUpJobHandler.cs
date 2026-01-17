using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class PaymentsCompleteTopUpJobHandler
{
    private readonly HttpClient _http;          // Payments API
    private readonly HttpClient _workflowHttp;  // Workflow API

    public PaymentsCompleteTopUpJobHandler(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Payments");

        // ✅ Workflow API client (bozmayalım diye named client şart koşmuyoruz)
        _workflowHttp = httpClientFactory.CreateClient();

        var workflowBaseUrl =
            Environment.GetEnvironmentVariable("WORKFLOW_API_BASEURL")
            ?? Environment.GetEnvironmentVariable("WorkflowApi__BaseUrl")
            ?? "http://host.docker.internal:7253";

        workflowBaseUrl = workflowBaseUrl.TrimEnd('/');

        _workflowHttp.BaseAddress = new Uri(workflowBaseUrl);
        _workflowHttp.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = doc.RootElement;

            var correlationId = GetString(root, "correlationId");

            bool creditOk = GetBool(root, "creditOk") ?? false;

            var errorCode = GetString(root, "errorCode");
            var errorMessage = GetString(root, "errorMessage");

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                await CompleteFail(client, job,
                    "PAYMENTS_COMPLETE_VALIDATION",
                    "correlationId zorunludur.",
                    "Complete TopUp");
                return;
            }

            // ✅ Swagger’a birebir: status STRING bekleniyor ("2","3","4")
            // Complete adımında: creditOk => "3" (Succeeded), değilse "4" (Failed)
            var status = creditOk ? "3" : "4";

            var request = new
            {
                correlationId,
                status,          // ✅ string
                errorCode,
                errorMessage
            };

            var resp = await _http.PostAsJsonAsync("/api/payments/status", request);

            if (!resp.IsSuccessStatusCode)
            {
                var bodyText = await resp.Content.ReadAsStringAsync();
                await CompleteFail(client, job,
                    "PAYMENTS_COMPLETE_HTTP_ERROR",
                    $"Payments status HTTP {(int)resp.StatusCode} | {bodyText}",
                    "Complete TopUp");
                return;
            }

            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            bool okResp =
                body.ValueKind == JsonValueKind.Object &&
                body.TryGetProperty("ok", out var okp) &&
                (okp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    ? okp.GetBoolean()
                    : true;

            // ✅ (EK) Workflow API ResultStore callback (TopUp)
            // ❗ Bu callback fail olursa akışı patlatmıyoruz, sadece logluyoruz.
            await TrySendWorkflowResultCallbackAsync(root, correlationId, okResp);

            // ✅ BPMN output isimleri paneldekiyle aynı
            var completeVars = new
            {
                persistOk = okResp,
                paymentsStatusUpdated = okResp,
                errorCode = okResp ? (string?)null : "PAYMENTS_STATUS_UPDATE_FAILED",
                errorMessage = okResp ? (string?)null : "Payments status update başarısız.",
                failedAtStep = okResp ? (string?)null : "Complete TopUp"
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars))
                .Send();
        }
        catch (Exception ex)
        {
            await CompleteFail(client, job,
                "PAYMENTS_COMPLETE_EXCEPTION",
                ex.Message,
                "Complete TopUp");
        }
    }

    private async Task TrySendWorkflowResultCallbackAsync(JsonElement root, string correlationId, bool okResp)
    {
        try
        {
            // Operate’ta gördüğün alanları “receipt” gibi minimal toparlıyoruz
            var data = new
            {
                accountId = GetString(root, "accountId"),
                customerId = GetString(root, "customerId"),
                amount = GetDecimal(root, "amount"),
                currency = GetString(root, "currency"),
                balanceBefore = GetDecimal(root, "balanceBefore"),
                balanceAfter = GetDecimal(root, "balanceAfter"),
                accountingTxId = GetString(root, "accountingTxId"),
                referenceId = GetString(root, "referenceId"),
                idempotencyKey = GetString(root, "idempotencyKey")
            };

            var payload = new
            {
                correlationId,
                success = okResp,
                status = okResp ? "Succeeded" : "Failed",
                message = okResp ? "TopUp successful" : "TopUp failed",
                data
            };

            var url = "/api/v1/payments/topup/result";

            Console.WriteLine($"[TopUp] callback -> {_workflowHttp.BaseAddress}{url} | correlationId={correlationId}");

            var resp = await _workflowHttp.PostAsJsonAsync(url, payload);
            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                Console.WriteLine($"[TopUp] CALLBACK HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} | body={raw}");
            else
                Console.WriteLine($"[TopUp] CALLBACK OK | body={raw}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TopUp] CALLBACK FAILED: {ex.GetType().Name} - {ex.Message}");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            persistOk = false,
            paymentsStatusUpdated = false,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // ----------------- helpers -----------------

    private static string? GetString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;

        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.Number => p.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => p.GetRawText()
        };
    }

    private static bool? GetBool(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;

        if (p.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return p.GetBoolean();

        if (p.ValueKind == JsonValueKind.String && bool.TryParse(p.GetString(), out var b))
            return b;

        return null;
    }

    private static decimal? GetDecimal(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;

        if (p.ValueKind == JsonValueKind.Number)
        {
            if (p.TryGetDecimal(out var d)) return d;
            if (p.TryGetDouble(out var dd)) return (decimal)dd;
            return null;
        }

        if (p.ValueKind == JsonValueKind.String)
        {
            var s = p.GetString();
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out d)) return d;
        }

        return null;
    }
}
