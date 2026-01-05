using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class TopUpValidateJobHandler
{
    private readonly ILogger<TopUpValidateJobHandler> _logger;

    public TopUpValidateJobHandler(ILogger<TopUpValidateJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        _logger.LogInformation("VALIDATE START | key={Key} | type={Type} | vars={Vars}",
            job.Key, job.Type, job.Variables);

        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
        var root = doc.RootElement;

        string? GetString(string name)
            => root.TryGetProperty(name, out var p) ? p.GetString() : null;

        decimal GetDecimal(string name)
        {
            if (!root.TryGetProperty(name, out var p)) return 0m;
            return p.ValueKind switch
            {
                JsonValueKind.Number => p.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(p.GetString(), out var d) ? d : 0m,
                _ => 0m
            };
        }

        var customerId = GetString("customerId");
        var accountId = GetString("accountId");
        var currency = GetString("currency");
        var idempotencyKey = GetString("idempotencyKey");
        var amount = GetDecimal("amount");

        bool validateOk = true;
        string? errorCode = null;
        string? errorMessage = null;

        if (string.IsNullOrWhiteSpace(customerId))
        {
            validateOk = false; errorCode = "VALIDATION_CUSTOMERID_MISSING"; errorMessage = "customerId is required";
        }
        else if (string.IsNullOrWhiteSpace(accountId))
        {
            validateOk = false; errorCode = "VALIDATION_ACCOUNTID_MISSING"; errorMessage = "accountId is required";
        }
        else if (amount <= 0)
        {
            validateOk = false; errorCode = "VALIDATION_AMOUNT_INVALID"; errorMessage = "amount must be greater than 0";
        }
        else if (string.IsNullOrWhiteSpace(currency))
        {
            validateOk = false; errorCode = "VALIDATION_CURRENCY_MISSING"; errorMessage = "currency is required";
        }
        else if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            validateOk = false; errorCode = "VALIDATION_IDEMPOTENCY_MISSING"; errorMessage = "idempotencyKey is required";
        }

        var outJson = JsonSerializer.Serialize(new
        {
            validateOk,
            errorCode,
            errorMessage,
            failedAtStep = validateOk ? (string?)null : "Validate Request"
        });

        await client.NewCompleteJobCommand(job.Key)
            .Variables(outJson)
            .Send();

        _logger.LogInformation("VALIDATE COMPLETE | key={Key} | ok={Ok}", job.Key, validateOk);
    }
}
