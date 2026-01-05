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
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = doc.RootElement;

            string? GetString(string name)
                => root.TryGetProperty(name, out var p) ? p.GetString() : null;

            int GetInt(string name, int def)
            {
                if (!root.TryGetProperty(name, out var p)) return def;
                try
                {
                    return p.ValueKind switch
                    {
                        JsonValueKind.Number => p.GetInt32(),
                        JsonValueKind.String => int.TryParse(p.GetString(), out var v) ? v : def,
                        _ => def
                    };
                }
                catch { return def; }
            }

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

            var accountId = GetString("accountId");
            var currency = GetString("currency");
            var amount = GetDecimal("amount");

            // ✅ Swagger'a göre defaultlar:
            // direction: 2 (credit)
            // operationType: 0 (topup)
            var direction = 2;
            var operationType = GetInt("operationType", 0);

            // ✅ referenceId GUID olmalı.
            // referenceId yoksa veya GUID değilse: yeni GUID üret.
            var referenceIdRaw = GetString("referenceId") ?? GetString("idempotencyKey");
            var referenceId = Guid.TryParse(referenceIdRaw, out var gid) ? gid.ToString() : Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(accountId) || amount <= 0 || string.IsNullOrWhiteSpace(currency))
            {
                await CompleteFail(client, job,
                    "ACCOUNTING_CREDIT_VALIDATION",
                    "accountId/amount/currency zorunludur.",
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
                var bodyText = await resp.Content.ReadAsStringAsync();
                await CompleteFail(client, job,
                    "ACCOUNTING_CREDIT_HTTP_ERROR",
                    $"HTTP {(int)resp.StatusCode} | {bodyText}",
                    "Credit Ledger");
                return;
            }

            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();

            // tolerant read
            string? txId =
                (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("id", out var id) ? id.GetString() : null)
                ?? (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("transactionId", out var tid) ? tid.GetString() : null)
                ?? (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("accountingTxId", out var atx) ? atx.GetString() : null);

            decimal? balanceBefore =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("balanceBefore", out var bb) && bb.ValueKind == JsonValueKind.Number
                    ? bb.GetDecimal()
                    : null;

            decimal? balanceAfter =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("balanceAfter", out var ba) && ba.ValueKind == JsonValueKind.Number
                    ? ba.GetDecimal()
                    : null;

            string? createdAt =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("createdAt", out var ca) ? ca.ToString()
                : (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("ledgerCreatedAt", out var lca) ? lca.ToString()
                : null);

            int ledgerOp =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("operationType", out var rop) && rop.ValueKind == JsonValueKind.Number
                    ? rop.GetInt32()
                    : operationType;

            string ledgerRef =
                body.ValueKind == JsonValueKind.Object && body.TryGetProperty("referenceId", out var rr) && rr.ValueKind == JsonValueKind.String
                    ? rr.GetString()!
                    : referenceId;

            // ✅ BPMN OUTPUT isimleriyle birebir + referenceId'yi de workflow'a yaz (tutarlılık için)
            var completeVars = new
            {
                creditOk = true,
                accountingTxId = txId,
                balanceBefore,
                balanceAfter,
                ledgerCreatedAt = createdAt,
                ledgerReferenceId = ledgerRef,
                ledgerOperationType = ledgerOp,

                // bu da çok faydalı: sonraki adımlar ve debug için
                referenceId = ledgerRef,
                direction,
                operationType,

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
            accountingTxId = (string?)null,
            balanceBefore = (decimal?)null,
            balanceAfter = (decimal?)null,
            ledgerCreatedAt = (string?)null,
            ledgerReferenceId = (string?)null,
            ledgerOperationType = (int?)null,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }
}