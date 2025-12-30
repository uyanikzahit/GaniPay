using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Validation.Worker;

public sealed class WorkerHost : IHostedService
{
    private readonly ILogger<WorkerHost> _logger;

    private IZeebeClient? _client;
    private readonly List<IDisposable> _openedWorkers = new();

    public WorkerHost(ILogger<WorkerHost> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Local Zeebe (Docker / Camunda 8 local)
        var gatewayAddress = "127.0.0.1:26500";

        _logger.LogInformation("Starting Zeebe workers. Gateway={Gateway}", gatewayAddress);

        _client = ZeebeClient.Builder()
            .UseGatewayAddress(gatewayAddress)
            .UsePlainText()
            .Build();

        // JOB TYPES (BPMN ile birebir aynı olmalı)
        OpenWorker("mock.device.info.get", HandleDeviceInfoGet);
        OpenWorker("mock.customer.phone.get", HandleCustomerPhoneGet);
        OpenWorker("mock.unlink.device", HandleUnlinkDevice);
        OpenWorker("real.customer.getById", HandleCustomerGetById);
        OpenWorker("mock.customer.getByEmail", HandleCustomerGetByEmail);
        OpenWorker("mock.sanction.query", HandleSanctionQuery);
        OpenWorker("mock.citizen.verification", HandleCitizenVerification);
        OpenWorker("mock.iban.verification", HandleIbanVerification);
        OpenWorker("real.address.verification", HandleAddressVerification);
        OpenWorker("mock.sim.check.control", HandleSimCheckControl);

        _logger.LogInformation("All Zeebe workers started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Zeebe workers...");

        foreach (var w in _openedWorkers)
        {
            try { w.Dispose(); } catch { /* ignore */ }
        }

        _openedWorkers.Clear();

        try { _client?.Dispose(); } catch { /* ignore */ }
        _client = null;

        _logger.LogInformation("Zeebe workers stopped.");
        return Task.CompletedTask;
    }

    private void OpenWorker(string jobType, JobHandler handler)
    {
        if (_client is null) throw new InvalidOperationException("Zeebe client not initialized.");

        var worker = _client.NewWorker()
            .JobType(jobType)
            .Handler(handler)
            .Name($"ganipay.validation.worker:{jobType}")
            .MaxJobsActive(32)
            .PollInterval(TimeSpan.FromMilliseconds(200))
            .Timeout(TimeSpan.FromSeconds(30))
            .Open();

        _openedWorkers.Add(worker);
        _logger.LogInformation("Worker opened: {JobType}", jobType);
    }

    // =========================================================
    // HANDLERS (IMPORTANT: return type VOID to match JobHandler)
    // =========================================================

    private void HandleDeviceInfoGet(IJobClient client, IJob job)
    {
        var vars = new
        {
            deviceInfo = new
            {
                ok = true,
                deviceId = ReadString(job, "deviceId") ?? "dev-001"
            }
        };

        Complete(client, job, vars, "DeviceInfoGet");
    }

    private void HandleCustomerPhoneGet(IJobClient client, IJob job)
    {
        var vars = new
        {
            customerPhone = new
            {
                ok = true,
                phoneNumber = ReadString(job, "phoneNumber") ?? "+905555555555"
            },
            // Senin BPMN'inde Customer Control gateway condition'ı genelde buradan besleniyor:
            customerControl = new { ok = true }
        };

        Complete(client, job, vars, "CustomerPhoneGet");
    }

    private void HandleUnlinkDevice(IJobClient client, IJob job)
    {
        var vars = new
        {
            unlinkDevice = new { ok = true }
        };

        Complete(client, job, vars, "UnlinkDevice");
    }

    private void HandleCustomerGetById(IJobClient client, IJob job)
    {
        var vars = new
        {
            customerGetById = new
            {
                ok = true,
                customerId = ReadString(job, "customerId") ?? Guid.NewGuid().ToString()
            }
        };

        Complete(client, job, vars, "CustomerGetById");
    }

    private void HandleCustomerGetByEmail(IJobClient client, IJob job)
    {
        var vars = new
        {
            customerGetByEmail = new
            {
                ok = true,
                email = ReadString(job, "email") ?? "test@ganipay.local"
            }
        };

        Complete(client, job, vars, "CustomerGetByEmail");
    }

    private void HandleSanctionQuery(IJobClient client, IJob job)
    {
        var vars = new
        {
            sanction = new { ok = true }
        };

        Complete(client, job, vars, "SanctionQuery");
    }

    private void HandleCitizenVerification(IJobClient client, IJob job)
    {
        var vars = new
        {
            citizenVerification = new { ok = true }
        };

        Complete(client, job, vars, "CitizenVerification");
    }

    private void HandleIbanVerification(IJobClient client, IJob job)
    {
        var vars = new
        {
            ibanVerification = new
            {
                ok = true,
                iban = ReadString(job, "iban") ?? "TR000000000000000000000000"
            }
        };

        Complete(client, job, vars, "IbanVerification");
    }

    private void HandleAddressVerification(IJobClient client, IJob job)
    {
        var vars = new
        {
            addressVerification = new { ok = true }
        };

        Complete(client, job, vars, "AddressVerification");
    }

    private void HandleSimCheckControl(IJobClient client, IJob job)
    {
        var vars = new
        {
            simCheck = new { ok = true }
        };

        Complete(client, job, vars, "SimCheckControl");
    }

    // =========================================================
    // COMPLETE / FAIL (sync wrapper)
    // =========================================================

    private void Complete(IJobClient client, IJob job, object variables, string stepName)
    {
        try
        {
            _logger.LogInformation("[{Step}] Completing job. key={Key}, type={Type}", stepName, job.Key, job.Type);

            var json = JsonSerializer.Serialize(variables);

            client.NewCompleteJobCommand(job.Key)
                .Variables(json)
                .Send()
                .GetAwaiter()
                .GetResult();

            _logger.LogInformation("[{Step}] Completed. key={Key}", stepName, job.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Step}] Failed to complete job. key={Key}, type={Type}", stepName, job.Key, job.Type);

            try
            {
                var nextRetries = Math.Max(job.Retries - 1, 0);

                client.NewFailCommand(job.Key)
                    .Retries(nextRetries)
                    .ErrorMessage(ex.Message)
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception failEx)
            {
                _logger.LogError(failEx, "[{Step}] Also failed to send FAIL command. key={Key}", stepName, job.Key);
            }
        }
    }

    private static string? ReadString(IJob job, string key)
    {
        if (string.IsNullOrWhiteSpace(job.Variables)) return null;

        try
        {
            using var doc = JsonDocument.Parse(job.Variables);
            if (!doc.RootElement.TryGetProperty(key, out var prop)) return null;
            return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
        }
        catch
        {
            return null;
        }
    }
}
