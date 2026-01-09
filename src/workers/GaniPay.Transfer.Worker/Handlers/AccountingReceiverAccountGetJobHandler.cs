using System;
using System.Linq;
using System.Text.Json;
using GaniPay.Transfer.Worker.Models;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class AccountingReceiverAccountGetJobHandler
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountingReceiverAccountGetJobHandler(IHttpClientFactory httpClientFactory)
        => _http = httpClientFactory.CreateClient("Accounting");

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var varsDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = varsDoc.RootElement;

            var receiverCustomerId = root.TryGetProperty("receiverCustomerId", out var p) ? p.GetString() : null;

            if (string.IsNullOrWhiteSpace(receiverCustomerId))
            {
                await CompleteFail(client, job,
                    code: "RECEIVER_GET_VALIDATION",
                    message: "receiverCustomerId zorunludur.",
                    step: "Receiver Account Get");
                return;
            }

            var resp = await _http.GetAsync($"/api/accounting/customers/{receiverCustomerId}/wallets");
            var bodyText = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"[RECEIVER_GET] {(int)resp.StatusCode} {resp.ReasonPhrase} GET /api/accounting/customers/{receiverCustomerId}/wallets");
            Console.WriteLine($"[RECEIVER_GET] BODY: {bodyText}");

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    code: "RECEIVER_GET_HTTP_ERROR",
                    message: $"HTTP {(int)resp.StatusCode} | {bodyText}",
                    step: "Receiver Account Get");
                return;
            }

            var dto = JsonSerializer.Deserialize<AccountingGetAccountResponse>(
                string.IsNullOrWhiteSpace(bodyText) ? "{}" : bodyText,
                JsonOpts
            ) ?? new AccountingGetAccountResponse();

            var selected = dto.Accounts?.FirstOrDefault();

            if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
            {
                await CompleteFail(client, job,
                    code: "RECEIVER_ACCOUNT_NOT_FOUND",
                    message: "Receiver için wallet/account bulunamadı.",
                    step: "Receiver Account Get");
                return;
            }

            Console.WriteLine($"[RECEIVER_SELECTED] id={selected.Id} bal={selected.Balance} status={selected.Status} curr={selected.Currency}");

            // ✅ BPMN Output Mapping’in beklediği GENERIC alanlar
            var completeVars = new
            {
                accountOk = true,
                accountId = selected.Id,
                balance = selected.Balance,
                errorCode = (string?)null,
                errorMessage = (string?)null
            };

            Console.WriteLine($"[RECEIVER_OUT] {JsonSerializer.Serialize(completeVars, JsonOpts)}");

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars, JsonOpts))
                .Send();
        }
        catch (Exception ex)
        {
            await CompleteFail(client, job,
                code: "RECEIVER_GET_EXCEPTION",
                message: ex.Message,
                step: "Receiver Account Get");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        // ✅ FAIL'de de GENERIC alanlar dönüyoruz
        var completeVars = new
        {
            accountOk = false,
            accountId = (string?)null,
            balance = (decimal?)null,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step
        };

        Console.WriteLine($"[RECEIVER_FAIL_OUT] {JsonSerializer.Serialize(completeVars, JsonOpts)}");

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars, JsonOpts))
            .Send();
    }
}
