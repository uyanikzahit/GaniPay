using System;
using System.Linq;
using System.Text.Json;
using GaniPay.Transfer.Worker.Models;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class AccountingSenderAccountGetJobHandler
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountingSenderAccountGetJobHandler(IHttpClientFactory httpClientFactory)
        => _http = httpClientFactory.CreateClient("Accounting");

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var varsDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = varsDoc.RootElement;

            var customerId = root.TryGetProperty("customerId", out var p) ? p.GetString() : null;

            if (string.IsNullOrWhiteSpace(customerId))
            {
                await CompleteFail(client, job,
                    code: "SENDER_GET_VALIDATION",
                    message: "customerId zorunludur.",
                    step: "Sender Account Get");
                return;
            }

            var resp = await _http.GetAsync($"/api/accounting/customers/{customerId}/wallets");
            var bodyText = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"[SENDER_GET] {(int)resp.StatusCode} {resp.ReasonPhrase} GET /api/accounting/customers/{customerId}/wallets");
            Console.WriteLine($"[SENDER_GET] BODY: {bodyText}");

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteFail(client, job,
                    code: "SENDER_GET_HTTP_ERROR",
                    message: $"HTTP {(int)resp.StatusCode} | {bodyText}",
                    step: "Sender Account Get");
                return;
            }

            var dto = JsonSerializer.Deserialize<AccountingGetAccountResponse>(
                string.IsNullOrWhiteSpace(bodyText) ? "{}" : bodyText,
                JsonOpts
            ) ?? new AccountingGetAccountResponse();

            // MVP: ilk account
            var selected = dto.Accounts?.FirstOrDefault();

            if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
            {
                await CompleteFail(client, job,
                    code: "SENDER_ACCOUNT_NOT_FOUND",
                    message: "Sender için wallet/account bulunamadı.",
                    step: "Sender Account Get");
                return;
            }

            Console.WriteLine($"[SENDER_SELECTED] id={selected.Id} bal={selected.Balance} status={selected.Status} curr={selected.Currency}");

            // ✅ BPMN Output Mapping’in beklediği GENERIC alanlar
            var completeVars = new
            {
                accountOk = true,
                accountId = selected.Id,
                balance = selected.Balance,
                errorCode = (string?)null,
                errorMessage = (string?)null
            };

            Console.WriteLine($"[SENDER_OUT] {JsonSerializer.Serialize(completeVars, JsonOpts)}");

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars, JsonOpts))
                .Send();
        }
        catch (Exception ex)
        {
            await CompleteFail(client, job,
                code: "SENDER_GET_EXCEPTION",
                message: ex.Message,
                step: "Sender Account Get");
        }
    }

    private static async Task CompleteFail(IJobClient client, IJob job, string code, string message, string step)
    {
        // ✅ FAIL'de de GENERIC alanlar dönüyoruz (BPMN mapping bozulmasın)
        var completeVars = new
        {
            accountOk = false,
            accountId = (string?)null,
            balance = (decimal?)null,
            errorCode = code,
            errorMessage = message,
            failedAtStep = step // opsiyonel debug
        };

        Console.WriteLine($"[SENDER_FAIL_OUT] {JsonSerializer.Serialize(completeVars, JsonOpts)}");

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars, JsonOpts))
            .Send();
    }
}
