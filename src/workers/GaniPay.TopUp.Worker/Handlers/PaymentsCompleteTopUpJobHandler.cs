using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class PaymentsCompleteTopUpJobHandler
{
    private readonly HttpClient _http;

    public PaymentsCompleteTopUpJobHandler(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Payments");
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(job.Variables);
            var root = doc.RootElement;

            var correlationId = root.TryGetProperty("correlationId", out var c) ? c.GetString() : null;

            bool creditOk = root.TryGetProperty("creditOk", out var ok) && ok.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? ok.GetBoolean()
                : false;

            var errorCode = root.TryGetProperty("errorCode", out var ec) ? ec.GetString() : null;
            var errorMessage = root.TryGetProperty("errorMessage", out var em) ? em.GetString() : null;

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                await CompleteFail(client, job,
                    "PAYMENTS_COMPLETE_VALIDATION",
                    "correlationId zorunludur.",
                    "Complete TopUp");
                return;
            }

            var status = creditOk ? "Success" : "Failed";

            var request = new
            {
                correlationId,
                status,
                errorCode,
                errorMessage
            };

            var resp = await _http.PostAsJsonAsync("/api/payments/status", request);

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    "PAYMENTS_COMPLETE_HTTP_ERROR",
                    $"Payments status HTTP {(int)resp.StatusCode}",
                    "Complete TopUp");
                return;
            }

            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            bool okResp =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("ok", out var okp) &&
                (okp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    ? okp.GetBoolean()
                    : true;

            var completeVars = new
            {
                persistOk = okResp,
                ok = okResp, // BPMN output -> paymentsStatusUpdated
                errorCode = okResp ? null : "PAYMENTS_STATUS_UPDATE_FAILED",
                errorMessage = okResp ? null : "Payments status update başarısız.",
                failedAtStep = okResp ? null : "Complete TopUp"
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

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            persistOk = false,
            ok = false,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}
