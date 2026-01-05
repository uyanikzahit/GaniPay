using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class AccountingCreditLedgerJobHandler
{
    private readonly HttpClient _http;

    public AccountingCreditLedgerJobHandler(IHttpClientFactory httpClientFactory)
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
            var currency = root.TryGetProperty("currency", out var cu) ? cu.GetString() : null;
            var referenceId = root.TryGetProperty("referenceId", out var r) ? r.GetString() : null;

            int direction = root.TryGetProperty("direction", out var d) && d.ValueKind == JsonValueKind.Number ? d.GetInt32() : 1;
            int operationType = root.TryGetProperty("operationType", out var op) && op.ValueKind == JsonValueKind.Number ? op.GetInt32() : 1;

            decimal amount = 0;
            if (root.TryGetProperty("amount", out var am) && am.ValueKind == JsonValueKind.Number)
                amount = am.GetDecimal();

            if (string.IsNullOrWhiteSpace(accountId) || amount <= 0 || string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(referenceId))
            {
                await CompleteFail(client, job,
                    "ACCOUNTING_CREDIT_VALIDATION",
                    "accountId/amount/currency/referenceId zorunludur.",
                    "Credit Ledger");
                return;
            }

            var request = new
            {
                accountId,
                direction,
                amount,
                currency,
                operationType,
                referenceId
            };

            var resp = await _http.PostAsJsonAsync("/api/accounting/transactions", request);

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    "ACCOUNTING_CREDIT_HTTP_ERROR",
                    $"Accounting transactions HTTP {(int)resp.StatusCode}",
                    "Credit Ledger");
                return;
            }

            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            // Alanlar yoksa bile akış bozulmasın
            string? txId = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("id", out var id) ? id.GetString() : null;

            decimal? balanceBefore = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("balanceBefore", out var bb) && bb.ValueKind == JsonValueKind.Number
                ? bb.GetDecimal()
                : null;

            decimal? balanceAfter = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("balanceAfter", out var ba) && ba.ValueKind == JsonValueKind.Number
                ? ba.GetDecimal()
                : null;

            string? createdAt = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("createdAt", out var ca) ? ca.ToString() : null;
            string? respRef = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("referenceId", out var rr) ? rr.GetString() : referenceId;

            int? respOp = body.ValueKind == JsonValueKind.Object && body.TryGetProperty("operationType", out var rop) && rop.ValueKind == JsonValueKind.Number
                ? rop.GetInt32()
                : operationType;

            var completeVars = new
            {
                creditOk = true,
                id = txId,
                balanceBefore = balanceBefore,
                balanceAfter = balanceAfter,
                createdAt = createdAt,
                referenceId = respRef,
                operationType = respOp,
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
                "ACCOUNTING_CREDIT_EXCEPTION",
                ex.Message,
                "Credit Ledger");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            creditOk = false,
            id = (string?)null,
            balanceBefore = (decimal?)null,
            balanceAfter = (decimal?)null,
            createdAt = (string?)null,
            referenceId = (string?)null,
            operationType = (int?)null,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}
