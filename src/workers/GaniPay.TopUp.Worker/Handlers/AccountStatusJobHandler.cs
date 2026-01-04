using System.Net.Http.Json;
using GaniPay.TopUp.Worker.Models;
using Microsoft.Extensions.Logging;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class AccountStatusJobHandler
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<AccountStatusJobHandler> _log;

    public AccountStatusJobHandler(IHttpClientFactory http, ILogger<AccountStatusJobHandler> log)
    {
        _http = http;
        _log = log;
    }

    public async Task Handle(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);

        // Accounting endpoint:
        // GET /api/accounting/accounts/status?accountId=...&customerId=...&currency=TRY
        var http = _http.CreateClient("accounting");
        var url =
            $"/api/accounting/accounts/status?accountId={vars.AccountId}&customerId={vars.CustomerId}&currency={vars.Currency}";

        AccountingAccountStatusResponse? resp = null;

        try
        {
            resp = await http.GetFromJsonAsync<AccountingAccountStatusResponse>(url);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Account status check failed (http).");
        }

        var isActive = resp is not null && resp.Status == 1;

        vars.AccountOk = isActive;
        if (!isActive)
        {
            vars.ErrorCode = "ACCOUNT_NOT_ACTIVE";
            vars.ErrorMessage = resp is null
                ? "Accounting account status alınamadı."
                : $"Account status aktif değil. Status={resp.Status}";
        }
        else
        {
            vars.ErrorCode = null;
            vars.ErrorMessage = null;
        }

        await Complete(job, client, new
        {
            accountOk = vars.AccountOk,
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
