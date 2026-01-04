using GaniPay.TopUp.Worker.Models;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class TopUpMockJobHandler
{
    // Aynı handler’ı 3 farklı jobType’a bağlarken “hangi step” olduğunu jobType’tan okuyoruz.
    public async Task Handle(dynamic client, dynamic job)
    {
        var vars = ReadVars(job);
        var jobType = (string)job.getType();

        // default: true
        var ok = true;

        // İstersen burada küçük bir profesyonel dokunuş:
        // amount çok büyükse provider fail simüle et vb.
        if (jobType == "mock.topup.provider.charge" && vars.Amount > 1_000_000)
            ok = false;

        if (jobType == "topup.limit.check")
        {
            vars.LimitOk = ok;
            if (!ok) { vars.ErrorCode = "LIMIT_EXCEEDED"; vars.ErrorMessage = "Mock limit fail."; }
            await Complete(job, client, new { limitOk = vars.LimitOk, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        if (jobType == "mock.topup.provider.charge")
        {
            vars.ProviderOk = ok;
            if (!ok) { vars.ErrorCode = "PROVIDER_DECLINED"; vars.ErrorMessage = "Mock provider declined."; }
            await Complete(job, client, new { providerOk = vars.ProviderOk, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        if (jobType == "mock.topup.notify.send")
        {
            vars.NotifyOk = ok;
            if (!ok) { vars.ErrorCode = "NOTIFY_FAILED"; vars.ErrorMessage = "Mock notify failed."; }
            await Complete(job, client, new { notifyOk = vars.NotifyOk, errorCode = vars.ErrorCode, errorMessage = vars.ErrorMessage });
            return;
        }

        // fallback: just complete
        await Complete(job, client, new { ok = true });
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
