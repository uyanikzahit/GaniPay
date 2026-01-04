using System.Net.Http.Json;
using GaniPay.TopUp.Worker.Models;
using Microsoft.Extensions.Logging;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class AccountingCreditLedgerJobHandler
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<AccountingCreditLedgerJobHandler> _log;

    public AccountingCreditLedgerJobHandler(IHttpClientFactory http, ILogger<AccountingCreditLedgerJobHandler> log)
    {
        _http = http;
        _log = log;
    }

    public async Task Handle(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);

        // POST /api/accounting/transactions
        // Senin swagger örneğine göre:
        // customerId, accountId, direction, amount, currency, operationType, referenceId
        var http = _http.CreateClient("accounting");

        var req = new
        {
            customerId = vars.CustomerId,
            accountId = vars.AccountId,
            direction = 2,               // sende direction=2 credit görünmüştü
            amount = vars.Amount,
            currency = vars.Currency,
            operationType = 1,           // sende operationType=1
            referenceId = vars.CorrelationId ?? Guid.NewGuid().ToString()
        };

        AccountingPostTransactionResponse? resp = null;

        try
        {
            var res = await http.PostAsJsonAsync("/api/accounting/transactions", req);
            res.EnsureSuccessStatusCode();
            resp = await res.Content.ReadFromJsonAsync<AccountingPostTransactionResponse>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Accounting credit ledger failed.");
        }

        var ok = resp is not null && !string.IsNullOrWhiteSpace(resp.Id);

        vars.CreditOk = ok;
        if (!ok)
        {
            vars.ErrorCode = "CREDIT_LEDGER_FAILED";
            vars.ErrorMessage = "Accounting transaction kaydı atılamadı.";
        }
        else
        {
            vars.AccountingTxId = resp!.Id;
            vars.BalanceAfter = resp.BalanceAfter;
            vars.ErrorCode = null;
            vars.ErrorMessage = null;
        }

        await Complete(job, client, new
        {
            creditOk = vars.CreditOk,
            accountingTxId = vars.AccountingTxId,
            balanceAfter = vars.BalanceAfter,
            errorCode = vars.ErrorCode,
            errorMessage = vars.ErrorMessage
        });
    }

    private static TopUpVariables ReadVars(dynamic job)
    {
        try
        {
            var json = (string)job.getVariables();
            return System.Text.Json.JsonSerializer.Deserialize<TopUpVariables>(json)!;
        }
        catch { return new TopUpVariables(); }
    }

    private static Task Complete(dynamic job, dynamic client, object variables)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(variables);
        return client.NewCompleteJobCommand(job.getKey()).Variables(json).Send();
    }
}
