using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class TopUpMockJobHandler
{
    public async Task Handle(IJobClient client, IJob job)
    {
        // job.Type üzerinden hangi mock olduğunu anlıyoruz
        var type = job.Type;

        object vars = type switch
        {
            "topup.limit.check" => new
            {
                limitOk = true,
                limitRemaining = 10_000m,
                limitUsed = 0m,
                limitTotal = 10_000m,
                errorCode = (string?)null,
                errorMessage = (string?)null,
                failedAtStep = (string?)null
            },

            "mock.topup.provider.charge" => new
            {
                providerOk = true,
                providerStatus = "Charged",
                providerRef = $"prov_{Guid.NewGuid():N}",
                errorCode = (string?)null,
                errorMessage = (string?)null,
                failedAtStep = (string?)null
            },

            "mock.topup.notify.send" => new
            {
                notifyOk = true,
                notificationId = $"ntf_{Guid.NewGuid():N}",
                errorCode = (string?)null,
                errorMessage = (string?)null,
                failedAtStep = (string?)null
            },

            _ => new
            {
                errorCode = "MOCK_UNKNOWN_JOBTYPE",
                errorMessage = $"Unknown mock jobType: {type}",
                failedAtStep = "Mock Handler"
            }
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(vars))
            .Send();
    }
}
