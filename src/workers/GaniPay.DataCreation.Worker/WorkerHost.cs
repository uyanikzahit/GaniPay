using GaniPay.DataCreation.Worker.Handlers;
using GaniPay.DataCreation.Worker.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.DataCreation.Worker;

public sealed class WorkerHost : IHostedService
{
    private readonly ILogger<WorkerHost> _logger;
    private readonly WorkerOptions _options;
    private readonly DataCreationJobHandlers _handlers;

    private IZeebeClient? _client;

    // ✅ GC fix
    private readonly List<IDisposable> _openedWorkers = new();

    public WorkerHost(
        ILogger<WorkerHost> logger,
        IOptions<WorkerOptions> options,
        DataCreationJobHandlers handlers)
    {
        _logger = logger;
        _options = options.Value;
        _handlers = handlers;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting DataCreation WorkerHost...");
        _logger.LogInformation("GatewayAddress={Gateway}", _options.GatewayAddress);

        _client = ZeebeClient.Builder()
            .UseGatewayAddress(_options.GatewayAddress)
            .UsePlainText()
            .Build();

        OpenWorkers();

        _logger.LogInformation("All DataCreation workers started.");
        return Task.CompletedTask;
    }

    private void OpenWorkers()
    {
        if (_client is null) throw new InvalidOperationException("Zeebe client not initialized.");

        var timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        var pollInterval = TimeSpan.FromMilliseconds(_options.PollIntervalMs);

        _openedWorkers.Add(Open("accounting.wallet.ledgerid.get", _handlers.HandleWalletAccountLedgerIdGet, timeout, pollInterval));
        _openedWorkers.Add(Open("customer.create", _handlers.HandleCustomerCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("customer.individual.create", _handlers.HandleCustomerIndividualCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("customer.phone.create", _handlers.HandleCustomerPhoneCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("mock.device.customer.link", _handlers.HandleLinkDeviceAndCustomer, timeout, pollInterval));
        _openedWorkers.Add(Open("accounting.account.create", _handlers.HandleAccountCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("customer.email.create", _handlers.HandleCustomerEmailCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("customer.address.create", _handlers.HandleCustomerAddressCreate, timeout, pollInterval));
        _openedWorkers.Add(Open("mock.customer.occupation.create", _handlers.HandleCustomerOccupationCreate, timeout, pollInterval));
    }

    private IDisposable Open(
        string jobType,
        JobHandler handler,
        TimeSpan timeout,
        TimeSpan pollInterval)
    {
        if (_client is null) throw new InvalidOperationException("Zeebe client not initialized.");

        var worker = _client.NewWorker()
            .JobType(jobType)
            .Handler(handler) // ✅ senin pakette void handler bekleniyor
            .Name(_options.WorkerName)
            .MaxJobsActive(_options.MaxJobsActive)
            .Timeout(timeout)
            .PollInterval(pollInterval)
            .Open();

        _logger.LogInformation("Worker opened: {JobType}", jobType);
        return worker;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping DataCreation WorkerHost...");

        foreach (var w in _openedWorkers)
            w.Dispose();

        _openedWorkers.Clear();

        _client?.Dispose();
        _client = null;

        return Task.CompletedTask;
    }
}
