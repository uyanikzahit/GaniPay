using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class AccountingAccountLedgerJobHandler
{
    private readonly HttpClient _http;

    public AccountingAccountLedgerJobHandler(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Accounting");
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = doc.RootElement;

            string? GetString(string name)
                => root.TryGetProperty(name, out var p) ? p.GetString() : null;

            decimal GetDecimal(string name)
            {
                if (!root.TryGetProperty(name, out var p)) return 0m;
                try
                {
                    return p.ValueKind switch
                    {
                        JsonValueKind.Number => p.GetDecimal(),
                        JsonValueKind.String => decimal.TryParse(p.GetString(), out var d) ? d : 0m,
                        _ => 0m
                    };
                }
                catch { return 0m; }
            }

            var senderAccountId = GetString("senderAccountId");
            var receiverAccountId = GetString("receiverAccountId");
            var currency = GetString("currency") ?? "TRY";
            var amount = GetDecimal("amount");

            // correlationId workflow’dan gelmeyebilir; gelmezse üretelim
            var correlationId = GetString("correlationId");
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            // Validation
            if (string.IsNullOrWhiteSpace(senderAccountId) ||
                string.IsNullOrWhiteSpace(receiverAccountId) ||
                amount <= 0m ||
                string.IsNullOrWhiteSpace(currency))
            {
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_VALIDATION",
                    message: "senderAccountId/receiverAccountId/amount/currency zorunludur.",
                    step: "Account Ledger");
                return;
            }

            // ✅ Accounting API beklediği request (Swagger örneğine göre)
            // direction:
            //   debit  (sender)
            //   credit (receiver)
            // operationType: transfer için örnek olarak 3 kullandın (ekrandaki)
            const int operationTypeTransfer = 3;

            var referenceId = correlationId; // idempotency / korelasyon için aynı id mantıklı

            // 1) Sender debit
            var senderReq = new
            {
                accountId = senderAccountId,
                direction = "debit",
                amount,
                currency,
                operationType = operationTypeTransfer,
                referenceId
            };

            var senderResp = await _http.PostAsJsonAsync("/api/accounting/transactions", senderReq);
            if (!senderResp.IsSuccessStatusCode)
            {
                var bodyText = await senderResp.Content.ReadAsStringAsync();
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_SENDER_HTTP_ERROR",
                    message: $"Sender debit HTTP {(int)senderResp.StatusCode} | {bodyText}",
                    step: "Account Ledger");
                return;
            }

            var senderBody = await senderResp.Content.ReadFromJsonAsync<JsonElement>();

            // 2) Receiver credit
            var receiverReq = new
            {
                accountId = receiverAccountId,
                direction = "credit",
                amount,
                currency,
                operationType = operationTypeTransfer,
                referenceId
            };

            var receiverResp = await _http.PostAsJsonAsync("/api/accounting/transactions", receiverReq);
            if (!receiverResp.IsSuccessStatusCode)
            {
                var bodyText = await receiverResp.Content.ReadAsStringAsync();
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_RECEIVER_HTTP_ERROR",
                    message: $"Receiver credit HTTP {(int)receiverResp.StatusCode} | {bodyText}",
                    step: "Account Ledger");
                return;
            }

            var receiverBody = await receiverResp.Content.ReadFromJsonAsync<JsonElement>();

            // tolerant parse (debug için)
            string? senderTxId =
                senderBody.ValueKind == JsonValueKind.Object && senderBody.TryGetProperty("id", out var sid) ? sid.GetString() : null;

            string? receiverTxId =
                receiverBody.ValueKind == JsonValueKind.Object && receiverBody.TryGetProperty("id", out var rid) ? rid.GetString() : null;

            decimal? senderBalanceBefore =
                senderBody.ValueKind == JsonValueKind.Object && senderBody.TryGetProperty("balanceBefore", out var sbb) && sbb.ValueKind == JsonValueKind.Number
                    ? sbb.GetDecimal()
                    : null;

            decimal? senderBalanceAfter =
                senderBody.ValueKind == JsonValueKind.Object && senderBody.TryGetProperty("balanceAfter", out var sba) && sba.ValueKind == JsonValueKind.Number
                    ? sba.GetDecimal()
                    : null;

            decimal? receiverBalanceBefore =
                receiverBody.ValueKind == JsonValueKind.Object && receiverBody.TryGetProperty("balanceBefore", out var rbb) && rbb.ValueKind == JsonValueKind.Number
                    ? rbb.GetDecimal()
                    : null;

            decimal? receiverBalanceAfter =
                receiverBody.ValueKind == JsonValueKind.Object && receiverBody.TryGetProperty("balanceAfter", out var rba) && rba.ValueKind == JsonValueKind.Number
                    ? rba.GetDecimal()
                    : null;

            // ✅ BPMN OUTPUT + bonus debug alanları
            var completeVars = new
            {
                ledgerOk = true,
                errorCode = (string?)null,
                errorMessage = (string?)null,

                // bonus
                correlationId,
                referenceId,
                senderLedgerTxId = senderTxId,
                receiverLedgerTxId = receiverTxId,
                senderBalanceBefore,
                senderBalanceAfter,
                receiverBalanceBefore,
                receiverBalanceAfter,
                failedAtStep = (string?)null
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars))
                .Send();
        }
        catch (Exception ex)
        {
            await CompleteFail(client, job,
                code: "ACCOUNT_LEDGER_EXCEPTION",
                message: ex.Message,
                step: "Account Ledger");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        var completeVars = new
        {
            ledgerOk = false,
            errorCode = code,
            errorMessage = message,

            // bonus
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}
