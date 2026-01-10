using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class AccountingAccountLedgerJobHandler
{
    private readonly HttpClient _http;

    public AccountingAccountLedgerJobHandler(IHttpClientFactory httpClientFactory)
        => _http = httpClientFactory.CreateClient("Accounting");

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

            int GetInt(string name, int @default)
            {
                if (!root.TryGetProperty(name, out var p)) return @default;
                try
                {
                    return p.ValueKind switch
                    {
                        JsonValueKind.Number => p.GetInt32(),
                        JsonValueKind.String => int.TryParse(p.GetString(), out var i) ? i : @default,
                        _ => @default
                    };
                }
                catch { return @default; }
            }

            // ✅ FALLBACK: Senin process’te sender için accountId var
            var senderAccountId =
                GetString("senderAccountId")
                ?? GetString("accountId"); // Sender Account Get’in bıraktığı

            // ✅ Receiver tarafında çoğu akışta receiverAccountId var; yoksa referenceId’yi de dene
            var receiverAccountId =
                GetString("receiverAccountId")
                ?? GetString("receiverAccountid")
                ?? GetString("referenceId"); // sen referansı receiver vermek istiyorsun

            var amount = GetDecimal("amount");
            var currency = GetString("currency") ?? "TRY";

            // operationType: BPMN’den gelsin; yoksa 3
            var operationType = GetInt("operationType", 3);

            // referenceId: sen “receiverAccountId verelim” dedin -> onu kullanıyoruz
            var referenceId = receiverAccountId;

            if (string.IsNullOrWhiteSpace(senderAccountId) ||
                string.IsNullOrWhiteSpace(receiverAccountId) ||
                string.IsNullOrWhiteSpace(referenceId) ||
                amount <= 0m)
            {
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_VALIDATION",
                    message: "senderAccountId/receiverAccountId/amount zorunludur.",
                    step: "Account Ledger");
                return;
            }

            // ✅ Swagger’a uygun: direction numeric (1=debit, 2=credit)
            var senderReq = new
            {
                accountId = senderAccountId,
                direction = 1,
                amount,
                currency,
                operationType,
                referenceId
            };

            var senderResp = await _http.PostAsJsonAsync("/api/accounting/transactions", senderReq);
            var senderText = await senderResp.Content.ReadAsStringAsync();
            if (!senderResp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_SENDER_HTTP_ERROR",
                    message: $"Sender debit HTTP {(int)senderResp.StatusCode} | {senderText}",
                    step: "Account Ledger");
                return;
            }

            var receiverReq = new
            {
                accountId = receiverAccountId,
                direction = 2,
                amount,
                currency,
                operationType,
                referenceId
            };

            var receiverResp = await _http.PostAsJsonAsync("/api/accounting/transactions", receiverReq);
            var receiverText = await receiverResp.Content.ReadAsStringAsync();
            if (!receiverResp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    code: "ACCOUNT_LEDGER_RECEIVER_HTTP_ERROR",
                    message: $"Receiver credit HTTP {(int)receiverResp.StatusCode} | {receiverText}",
                    step: "Account Ledger");
                return;
            }

            // OK
            var completeVars = new
            {
                ledgerOk = true,
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
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}