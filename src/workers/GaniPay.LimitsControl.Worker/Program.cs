using GaniPay.LimitsControl.Worker.Handlers;
using GaniPay.LimitsControl.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zeebe.Client;

var builder = Host.CreateApplicationBuilder(args);

// Options
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection("Zeebe"));
builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection("Services"));

// HttpClient (ileride gerçek API çağrısı)
builder.Services.AddHttpClient();

// Handlers
builder.Services.AddSingleton<WalletLimitDetailsGetJobHandler>();
builder.Services.AddSingleton<AccountsGetJobHandler>();
builder.Services.AddSingleton<CustomerLimitsEvaluationGetJobHandler>();
builder.Services.AddSingleton<WalletToWalletLimitsControlJobHandler>();

// Zeebe Client
builder.Services.AddSingleton<IZeebeClient>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<ZeebeOptions>>().Value;

    return ZeebeClient.Builder()
        .UseGatewayAddress(opt.GatewayAddress)
        .UsePlainText()
        .Build();
});

var host = builder.Build();

var zeebe = host.Services.GetRequiredService<IZeebeClient>();

var limitDefsHandler = host.Services.GetRequiredService<WalletLimitDetailsGetJobHandler>();
var accountsHandler = host.Services.GetRequiredService<AccountsGetJobHandler>();
var evalHandler = host.Services.GetRequiredService<CustomerLimitsEvaluationGetJobHandler>();
var w2wHandler = host.Services.GetRequiredService<WalletToWalletLimitsControlJobHandler>();

// BPMN job types
using var limitDefsWorker = zeebe.NewWorker()
    .JobType("wallet.limits.definitions.details.get")
    .Handler(limitDefsHandler.Handle)
    .Name("GaniPay.LimitsControl.Worker.limitdefs")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var accountsWorker = zeebe.NewWorker()
    .JobType("accounting.accounts.get")
    .Handler(accountsHandler.Handle)
    .Name("GaniPay.LimitsControl.Worker.accounts")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var evalWorker = zeebe.NewWorker()
    .JobType("customer.limits.evaluation.get")
    .Handler(evalHandler.Handle)
    .Name("GaniPay.LimitsControl.Worker.evaluation")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// senin bpmn’de son task job type hangisiyse onu dinle
using var w2wWorker = zeebe.NewWorker()
    .JobType("wallet.to.wallet.limits.control")
    .Handler(w2wHandler.Handle)
    .Name("GaniPay.LimitsControl.Worker.w2w")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();
