using System.Net.Http.Json;
using GaniPay.TopUp.Worker.Models;
using Microsoft.Extensions.Logging;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class PaymentsCompleteTopUpJobHandler
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<PaymentsCompleteTopUpJobHandler> _log;

    public PaymentsCompleteTopUpJobHandler(IHttpClientFactory http, ILogger<PaymentsCompleteTopUpJobHandler> log)
    {
        _http = http;
        _log = log;
    }

    public async Task Handle(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);

        // Bu task Credit OK'tan sonra geldiği için default Success
        // İstersen process variable’dan finalStatus da okuyabiliriz.
        short status = 3; // Succeeded
        if (!string.IsNullOrWhiteSpace(vars.ErrorCode))
            status = 4; // Failed

        var req = new
        {
            correlationId = vars.CorrelationId,
            status = status,
            errorCode = vars.ErrorCode,
            errorMessage = vars.ErrorMessage
        };

        var http = _http.CreateClient("payments");

        PaymentsStatusUpdateResponse? resp = null;

        try
        {
            var res = await http.PostAsJsonAsync("/api/payments/status", req);
            res.EnsureSuccessStatusCode();
            resp = await res.Content.ReadFromJsonAsync<PaymentsStatusUpdateResponse>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Payments status update failed.");
        }

        var ok = resp is not null && resp.Ok;

        vars.PersistOk = ok;
        if (!ok)
        {
            vars.ErrorCode = "PAYMENTS_STATUS_UPDATE_FAILED";
            vars.ErrorMessage = "Payment status update başarısız.";
        }

        await Complete(job, client, new
        {
            persistOk = vars.PersistOk,
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
