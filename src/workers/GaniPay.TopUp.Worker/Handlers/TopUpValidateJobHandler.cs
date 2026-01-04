using GaniPay.TopUp.Worker.Models;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class TopUpValidateJobHandler
{
    // Program.cs'te lambda ile çağıracağımız asıl method
    public async Task HandleJob(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);

        if (vars.CustomerId == Guid.Empty || vars.AccountId == Guid.Empty)
        {
            vars.IsValid = false;
            vars.ErrorCode = "VALIDATION_INVALID_ID";
            vars.ErrorMessage = "customerId/accountId boş olamaz.";
            await Complete(job, client, new { isValid = false, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        if (vars.Amount <= 0)
        {
            vars.IsValid = false;
            vars.ErrorCode = "VALIDATION_AMOUNT";
            vars.ErrorMessage = "amount 0'dan büyük olmalı.";
            await Complete(job, client, new { isValid = false, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        if (string.IsNullOrWhiteSpace(vars.Currency))
        {
            vars.IsValid = false;
            vars.ErrorCode = "VALIDATION_CURRENCY";
            vars.ErrorMessage = "currency boş olamaz.";
            await Complete(job, client, new { isValid = false, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        // success
        vars.IsValid = true;
        vars.ErrorCode = null;
        vars.ErrorMessage = null;

        await Complete(job, client, new
        {
            isValid = true,
            errorCode = (string?)null,
            errorMessage = (string?)null,
            customerId = vars.CustomerId,
            accountId = vars.AccountId,
            amount = vars.Amount,
            currency = vars.Currency,
            idempotencyKey = vars.IdempotencyKey
        });
    }

    private static TopUpVariables ReadVars(dynamic job)
    {
        try
        {
            var json = (string)job.getVariables();
            return System.Text.Json.JsonSerializer.Deserialize<TopUpVariables>(json)!;
        }
        catch
        {
            return new TopUpVariables();
        }
    }

    private static Task Complete(dynamic job, dynamic client, object variables)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(variables);
        return client.NewCompleteJobCommand(job.getKey()).Variables(json).Send();
    }
}
