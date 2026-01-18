using System.Text.Json;
using System.Net.Http.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Transfer.Worker.Handlers;

public sealed class TransferMockJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _workflowHttp;

    public TransferMockJobHandler(IHttpClientFactory httpClientFactory)
    {
        _workflowHttp = httpClientFactory.CreateClient();

        // Workflow API base url (Aspire/Local)
        var workflowBaseUrl =
            Environment.GetEnvironmentVariable("WORKFLOW_API_BASEURL")
            ?? Environment.GetEnvironmentVariable("WorkflowApi__BaseUrl")
            // Aspire’da senin Workflow API swagger’ı hangi porttaysa onu ver.
            // Şimdilik güvenli default: http 5210 (sen kullanıyorsun)
            ?? "http://localhost:5210";

        workflowBaseUrl = workflowBaseUrl.TrimEnd('/');
        _workflowHttp.BaseAddress = new Uri(workflowBaseUrl);
        _workflowHttp.Timeout = TimeSpan.FromSeconds(15);
    }

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

            // Debug / trace için faydalı
            var idempotencyKey = GetString("idempotencyKey");
            var referenceIdRaw = GetString("referenceId") ?? idempotencyKey;
            var referenceId = Guid.TryParse(referenceIdRaw, out var gid) ? gid.ToString() : Guid.NewGuid().ToString();

            // (NEW) correlationId'yi root'tan al (process start'tan geliyor)
            var correlationId = GetString("correlationId");

            // JobType’a göre mock davranışı
            object completeVars = job.Type switch
            {
                "transfer.receiver.resolve" => new
                {
                    receiverResolved = true,
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                "transfer.expense.get" => new
                {
                    expenseOk = true,
                    expenseDescription = "Expense calculated (mock)",
                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

                // ⚠️ BURAYA DOKUNMADIM (senin mock initiate davranışın aynı)
                "payments.transfer.initiate" => new
                {
                    correlationId = $"trf-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    status = "Running",
                    paymentsCorrelationId = $"trf-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    paymentsStatus = "Running",

                    referenceId,
                    idempotencyKey,

                    errorCode = (string?)null,
                    errorMessage = (string?)null,
                    failedAtStep = (string?)null
                },

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

                // ✅ Complete Transfer - callback burada
                "payments.transfer.complete" => BuildCompleteTransferVarsAndCallback(
                    root,
                    correlationId,
                    referenceId,
                    idempotencyKey,
                    GetBool("ledgerOk", true)
                ),

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

    /// <summary>
    /// "payments.transfer.complete" için hem BPMN output'u üretir,
    /// hem de Workflow API'ye sonucu callback eder (fail olsa bile akışı patlatmaz).
    /// </summary>
    private object BuildCompleteTransferVarsAndCallback(
        JsonElement root,
        string? correlationId,
        string referenceId,
        string? idempotencyKey,
        bool ledgerOk)
    {
        // BPMN için completeVars
        var status = ledgerOk ? "Succeeded" : "Failed";

        // Callback'i async atmamız gerekiyor ama switch içinde object dönüyoruz.
        // Bu yüzden "fire-and-forget" mantığıyla Task.Run yapıyoruz (akışı bloklamasın).
        _ = Task.Run(() => TrySendTransferResultCallbackAsync(root, correlationId, ledgerOk, status, referenceId, idempotencyKey));

        return new
        {
            persistOk = true,
            paymentsStatusUpdated = true,
            status,

            referenceId,
            idempotencyKey,

            errorCode = (string?)null,
            errorMessage = (string?)null,
            failedAtStep = (string?)null
        };
    }

    private async Task TrySendTransferResultCallbackAsync(
        JsonElement root,
        string? correlationId,
        bool ledgerOk,
        string status,
        string referenceId,
        string? idempotencyKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                Console.WriteLine("[Transfer] CALLBACK SKIPPED: correlationId is null/empty");
                return;
            }

            // ✅ client'a fazla veri açmayacağız: sadece message dönmek istiyorsun.
            // Workflow API store yine data alabilir ama biz minimal gönderiyoruz.
            var payload = new
            {
                correlationId,
                success = ledgerOk,
                status,
                message = ledgerOk ? "Transfer completed successfully." : "Transfer failed."
                // data göndermiyoruz
            };

            var url = "/api/v1/transfers/transfer/result";
            Console.WriteLine($"[Transfer] callback -> {_workflowHttp.BaseAddress}{url} | correlationId={correlationId}");

            var resp = await _workflowHttp.PostAsJsonAsync(url, payload);
            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                Console.WriteLine($"[Transfer] CALLBACK HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} | body={raw}");
            else
                Console.WriteLine($"[Transfer] CALLBACK OK | body={raw}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Transfer] CALLBACK FAILED: {ex.GetType().Name} - {ex.Message}");
        }
    }
}
