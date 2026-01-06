using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using GaniPay.LimitsControl.Worker.Models;

namespace GaniPay.LimitsControl.Worker.Handlers;

public sealed class CustomerLimitsEvaluationGetJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        // mock: sabit usage
        var limits = new CustomerLimits
        {
            DailyUsed = 1200,
            DailyRemaining = 18800
        };

        var output = new
        {
            customerLimitsOk = true,
            customerLimits = new { dailyUsed = limits.DailyUsed, dailyRemaining = limits.DailyRemaining }
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(output, JsonOpts))
            .Send();
    }
}
