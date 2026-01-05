using System;
using System.Threading;
using System.Threading.Tasks;
using GaniPay.TopUp.Worker.Handlers;
using GaniPay.TopUp.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeebe.Client;
using Zeebe.Client.Impl.Builder;

var builder = Host.CreateApplicationBuilder(args);

// Options
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection(ZeebeOptions.SectionName));
builder.Services.Configure<AccountingApiOptions>(builder.Configuration.GetSection(AccountingApiOptions.SectionName));
builder.Services.Configure<PaymentsApiOptions>(builder.Configuration.GetSection(PaymentsApiOptions.SectionName));

// HttpClients
builder.Services.AddHttpClient("Accounting", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<AccountingApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/'));
});

builder.Services.AddHttpClient("Payments", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<PaymentsApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/'));
});

// Handlers (senin düzenini bozmuyorum: Singleton)
builder.Services.AddSingleton<TopUpValidateJobHandler>();
builder.Services.AddSingleton<AccountStatusJobHandler>();
builder.Services.AddSingleton<PaymentsInitiateTopUpJobHandler>();
builder.Services.AddSingleton<AccountingCreditLedgerJobHandler>();
builder.Services.AddSingleton<PaymentsCompleteTopUpJobHandler>();
builder.Services.AddSingleton<TopUpMockJobHandler>();

// Zeebe Hosted Service
builder.Services.AddHostedService<ZeebeWorkerHost>();

var host = builder.Build();
await host.RunAsync();


// ---------------- Hosted Service ----------------
public sealed class ZeebeWorkerHost : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ZeebeWorkerHost> _logger;

    private IZeebeClient? _client;

    private IDisposable? _wValidate;
    private IDisposable? _wAccStatus;
    private IDisposable? _wLimit;
    private IDisposable? _wInitiate;
    private IDisposable? _wProvider;
    private IDisposable? _wCredit;
    private IDisposable? _wComplete;
    private IDisposable? _wNotify;

    public ZeebeWorkerHost(IServiceProvider sp, ILogger<ZeebeWorkerHost> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _sp.CreateScope();

        var zeebeOpt = scope.ServiceProvider.GetRequiredService<IOptions<ZeebeOptions>>().Value;

        _logger.LogInformation("ZeebeWorkerHost starting. Gateway={Gateway}", zeebeOpt.GatewayAddress);

        // Senin kullandığın Zeebe.Client sürümünde UsePlainText sonrası Build zinciri 2 aşamalı olabiliyor.
        // Bu yüzden en stabil yaklaşım:
        _client = ZeebeClient.Builder()
            .UseGatewayAddress(zeebeOpt.GatewayAddress)
            .UsePlainText()
            .Build();

        _logger.LogInformation("Zeebe client created. Opening workers...");

        // Resolve handlers
        var validate = scope.ServiceProvider.GetRequiredService<TopUpValidateJobHandler>();
        var accStatus = scope.ServiceProvider.GetRequiredService<AccountStatusJobHandler>();
        var mock = scope.ServiceProvider.GetRequiredService<TopUpMockJobHandler>();
        var initiate = scope.ServiceProvider.GetRequiredService<PaymentsInitiateTopUpJobHandler>();
        var credit = scope.ServiceProvider.GetRequiredService<AccountingCreditLedgerJobHandler>();
        var complete = scope.ServiceProvider.GetRequiredService<PaymentsCompleteTopUpJobHandler>();

        // Workers (JobType BPMN ile birebir)
        _wValidate = _client.NewWorker()
            .JobType("topup.validate")
            .Handler(async (c, j) =>
            {
                Console.WriteLine($"JOB ARRIVED | type={j.Type} | key={j.Key} | vars={j.Variables}");

                await c.NewCompleteJobCommand(j.Key).Send();

                Console.WriteLine($"JOB COMPLETED | key={j.Key}");
            })
            .Name("TopUp.Validate")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: topup.validate");

        _wAccStatus = _client.NewWorker()
            .JobType("accounting.account.status.check")
            .Handler(async (c, j) => await accStatus.Handle(c, j))
            .Name("TopUp.AccountStatus")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: accounting.account.status.check");

        _wLimit = _client.NewWorker()
            .JobType("topup.limit.check")
            .Handler(async (c, j) => await mock.Handle(c, j))
            .Name("TopUp.Limit.Mock")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: topup.limit.check");

        _wInitiate = _client.NewWorker()
            .JobType("payments.topup.initiate")
            .Handler(async (c, j) => await initiate.Handle(c, j))
            .Name("TopUp.Payments.Initiate")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: payments.topup.initiate");

        _wProvider = _client.NewWorker()
            .JobType("mock.topup.provider.charge")
            .Handler(async (c, j) => await mock.Handle(c, j))
            .Name("TopUp.Provider.Mock")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: mock.topup.provider.charge");

        _wCredit = _client.NewWorker()
            .JobType("accounting.topup.credit")
            .Handler(async (c, j) => await credit.Handle(c, j))
            .Name("TopUp.Accounting.Credit")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: accounting.topup.credit");

        _wComplete = _client.NewWorker()
            .JobType("payments.topup.complete")
            .Handler(async (c, j) => await complete.Handle(c, j))
            .Name("TopUp.Payments.Complete")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: payments.topup.complete");

        _wNotify = _client.NewWorker()
            .JobType("mock.topup.notify.send")
            .Handler(async (c, j) => await mock.Handle(c, j))
            .Name("TopUp.Notify.Mock")
            .MaxJobsActive(32)
            .Open();
        _logger.LogInformation("Opened worker: mock.topup.notify.send");

        // idle loop
        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ZeebeWorkerHost stopping...");

        _wValidate?.Dispose();
        _wAccStatus?.Dispose();
        _wLimit?.Dispose();
        _wInitiate?.Dispose();
        _wProvider?.Dispose();
        _wCredit?.Dispose();
        _wComplete?.Dispose();
        _wNotify?.Dispose();

        _client?.Dispose();

        await base.StopAsync(cancellationToken);
    }


}