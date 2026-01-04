using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.TopUp.Worker.Handlers;

public sealed class TopUpValidateJobHandler
{
    public async Task Handle(IJobClient client, IJob job)
    {
        // şimdilik basit validate (mock/placeholder)
        // job.Variables JSON string gelir
        var vars = job.Variables;

        // TODO: gerçek validasyon kuralları
        // örnek output: isValid + errorCode/message
        var output = new
        {
            validation = new
            {
                isValid = true,
                errorCode = (string?)null,
                errorMessage = (string?)null
            }
        };

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(output))
            .Send();
    }
}
