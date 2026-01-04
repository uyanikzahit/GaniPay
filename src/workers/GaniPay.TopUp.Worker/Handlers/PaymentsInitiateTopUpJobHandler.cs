using System.Net.Http.Json;
using GaniPay.TopUp.Worker.Models;
using Microsoft.Extensions.Logging;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class PaymentsInitiateTopUpJobHandler
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<PaymentsInitiateTopUpJobHandler> _log;

    public PaymentsInitiateTopUpJobHandler(IHttpClientFactory http, ILogger<PaymentsInitiateTopUpJobHandler> log)
    {
        _http = http;
        _log = log;
    }

    public async Task Handle(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);

        // POST /api/payments/topups
        // body: { customerId, amount, currency, idempotencyKey }
        var http = _http.CreateClient("payments");

        var req = new
        {
            customerId = vars.CustomerId,
            amount = vars.Amount,
            currency = vars.Currency,
            idempotencyKey = vars.IdempotencyKey ?? $"topup-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
        };

        PaymentsInitiateTopUpResponse? resp = null;

        try
        {
            var res = await http.PostAsJsonAsync("/api/payments/topups", req);
            res.EnsureSuccessStatusCode();
            resp = await res.Content.ReadFromJsonAsync<PaymentsInitiateTopUpResponse>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Payments initiate topup failed.");
        }

        var ok = resp is not null && !string.IsNullOrWhiteSpace(resp.CorrelationId);

        vars.OrderOk = ok;
        if (!ok)
        {
            vars.ErrorCode = "PAYMENTS_INITIATE_FAILED";
            vars.ErrorMessage = "TopUp initiate edilemedi.";
        }
        else
        {
            vars.CorrelationId = resp!.CorrelationId;
            vars.ErrorCode = null;
            vars.ErrorMessage = null;
        }

        await Complete(job, client, new
        {
            orderOk = vars.OrderOk,
            correlationId = vars.CorrelationId,
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
