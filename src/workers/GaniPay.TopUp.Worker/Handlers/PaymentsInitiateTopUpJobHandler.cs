using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class PaymentsInitiateTopUpJobHandler
{
    private readonly HttpClient _http;

    public PaymentsInitiateTopUpJobHandler(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Payments");
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(job.Variables);
            var root = doc.RootElement;

            var customerId = root.TryGetProperty("customerId", out var c) ? c.GetString() : null;
            var currency = root.TryGetProperty("currency", out var cu) ? cu.GetString() : null;
            var idempotencyKey = root.TryGetProperty("idempotencyKey", out var i) ? i.GetString() : null;

            decimal amount = 0;
            if (root.TryGetProperty("amount", out var a) && a.ValueKind == JsonValueKind.Number)
                amount = a.GetDecimal();

            if (string.IsNullOrWhiteSpace(customerId) || amount <= 0 || string.IsNullOrWhiteSpace(currency))
            {
                await CompleteFail(client, job,
                    "PAYMENTS_INITIATE_VALIDATION",
                    "TopUp initiate için customerId/amount/currency zorunludur.",
                    "Initiate TopUp");
                return;
            }

            var request = new
            {
                customerId,
                amount,
                currency,
                idempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString("N")
            };

            var resp = await _http.PostAsJsonAsync("/api/payments/topups", request);

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    "PAYMENTS_INITIATE_HTTP_ERROR",
                    $"Payments topups HTTP {(int)resp.StatusCode}",
                    "Initiate TopUp");
                return;
            }

            // Swagger response değişebilir; yoksa bile correlation üretelim
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            string paymentCorrelationId =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("correlationId", out var corr)
                    ? (corr.GetString() ?? Guid.NewGuid().ToString())
                    : Guid.NewGuid().ToString();

            string status =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("status", out var st)
                    ? (st.GetString() ?? "Initiated")
                    : "Initiated";

            var completeVars = new
            {
                orderOk = true,
                correlationId = paymentCorrelationId, // BPMN output -> paymentCorrelationId
                status = status,                      // BPMN output -> paymentStatus
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
                "PAYMENTS_INITIATE_EXCEPTION",
                ex.Message,
                "Initiate TopUp");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            orderOk = false,
            correlationId = (string?)null,
            status = "Failed",
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}
