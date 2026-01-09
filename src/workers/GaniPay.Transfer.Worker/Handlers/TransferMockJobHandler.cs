using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class TransferMockJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task Handle(IJobClient client, IJob job)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.Variables) ? "{}" : job.Variables);
            var root = doc.RootElement;

            string? GetString(string name)
                => root.TryGetProperty(name, out var p) ? p.GetString() : null;


            bool GetBool(string name, bool def = false)
            {
                if (!root.TryGetProperty(name, out var p)) return def;
                try
                {
                    return p.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.String => bool.TryParse(p.GetString(), out var b) ? b : def,
                        JsonValueKind.Number => p.GetInt32() != 0,
                        _ => def
                    };
                }
                catch { return def; }
            }

            // Debug / trace için faydalý
            var idempotencyKey = GetString("idempotencyKey");
            var referenceIdRaw = GetString("referenceId") ?? idempotencyKey;
            var referenceId = Guid.TryParse(referenceIdRaw, out var gid) ? gid.ToString() : Guid.NewGuid().ToString();

            // JobType’a göre mock davranýþý
            object completeVars = job.Type switch
            {
                // 1) Resolve Receiver (AML) - mock
                "transfer.receiver.resolve" => new
                {
                    receiverResolved = true,
                    // senin BPMN’de receiverResolved üzerinden gateway karar veriyor
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // 2) Expense Get - mock
                "transfer.expense.get" => new
                {
                    expenseOk = true,
                    expenseDescription = "Expense calculated (mock)",
                    // gateway expenseOk ile karar veriyor
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // 3) Initiate Transfer (Payments) - mock
                "payments.transfer.initiate" => new
                {
                    // BPMN’de Gw_InitiateOk “correlationId set?” üzerinden gidiyor
                    correlationId = $"trf-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    status = "Running", // istersen "Running" / "Accepted"
                    paymentsCorrelationId = $"trf-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    paymentsStatus = "Running",

                    // trace
                    referenceId,
                    idempotencyKey,

                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // 4) Notify (mock) - iki job type ayný handler
                "notification.send.sender" => new
                {
                    receiverNotifyMessageId = $"msg-receiver-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                "notification.send.receiver" => new
                {
                    senderNotifyMessageId = $"msg-sender-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // 5) Complete Transfer - mock
                "payments.transfer.complete" => new
                {
                    // senin BPMN’de complete sonrasý success event’e gidiyor (Flow_0kenwne)
                    persistOk = true,
                    paymentsStatusUpdated = true,
                    // istersen ledgerOk’a göre status bas
                    status = GetBool("ledgerOk", true) ? "Succeeded" : "Failed",

                    referenceId,
                    idempotencyKey,

                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // default: bilmediðimiz job type gelirse fail yerine complete edip trace bas (dev ortamý)
                _ => new
                {
                    errorCode = "MOCK_UNKNOWN_JOBTYPE",
                    errorMessage = $"Mock handler does not support jobType='{job.Type}'",
                    failedAtStep = "TransferMockJobHandler"
                }
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars, JsonOpts))
                .Send();
        }
        catch (Exception ex)
        {
            var failVars = new
            {
                errorCode = "MOCK_EXCEPTION",
                errorMessage = ex.Message,
                failedAtStep = "TransferMockJobHandler"
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(failVars, JsonOpts))
                .Send();
        }
    }
}