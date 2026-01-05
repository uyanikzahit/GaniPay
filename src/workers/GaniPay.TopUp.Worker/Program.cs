using System;
using GaniPay.TopUp.Worker.Handlers;
using GaniPay.TopUp.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zeebe.Client;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = Host.CreateApplicationBuilder(args);

// Options
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection(ZeebeOptions.SectionName));
builder.Services.Configure<AccountingApiOptions>(builder.Configuration.GetSection(AccountingApiOptions.SectionName));
builder.Services.Configure<PaymentsApiOptions>(builder.Configuration.GetSection(PaymentsApiOptions.SectionName));

// HttpClients
builder.Services.AddHttpClient("Accounting", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<AccountingApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/')); // ör: http://localhost:5103
});

builder.Services.AddHttpClient("Payments", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<PaymentsApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/'));
});

// Handlers
builder.Services.AddSingleton<TopUpValidateJobHandler>();
builder.Services.AddSingleton<AccountStatusJobHandler>();
builder.Services.AddSingleton<PaymentsInitiateTopUpJobHandler>();
builder.Services.AddSingleton<AccountingCreditLedgerJobHandler>();
builder.Services.AddSingleton<PaymentsCompleteTopUpJobHandler>();
builder.Services.AddSingleton<TopUpMockJobHandler>();

// Zeebe Client (Login worker ile aynı)
builder.Services.AddSingleton<IZeebeClient>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<ZeebeOptions>>().Value;

    return ZeebeClient.Builder()
        .UseGatewayAddress(opt.GatewayAddress) // 127.0.0.1:26500
        .UsePlainText()
        .Build();
});

var host = builder.Build();

// Resolve
var zeebe = host.Services.GetRequiredService<IZeebeClient>();

var validate = host.Services.GetRequiredService<TopUpValidateJobHandler>();
var accStatus = host.Services.GetRequiredService<AccountStatusJobHandler>();
var mock = host.Services.GetRequiredService<TopUpMockJobHandler>();
var initiate = host.Services.GetRequiredService<PaymentsInitiateTopUpJobHandler>();
var credit = host.Services.GetRequiredService<AccountingCreditLedgerJobHandler>();
var complete = host.Services.GetRequiredService<PaymentsCompleteTopUpJobHandler>();

// Workers (Login pattern: using var + Timeout)
using var wValidate = zeebe.NewWorker()
    .JobType("topup.validate")
    .Handler(validate.Handle)
    .Name("GaniPay.TopUp.Worker.validate")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wAccStatus = zeebe.NewWorker()
    .JobType("accounting.account.status.check")
    .Handler(accStatus.Handle)
    .Name("GaniPay.TopUp.Worker.accountStatus")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wLimit = zeebe.NewWorker()
    .JobType("topup.limit.check")
    .Handler(mock.Handle)
    .Name("GaniPay.TopUp.Worker.limit")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wInitiate = zeebe.NewWorker()
    .JobType("payments.topup.initiate")
    .Handler(initiate.Handle)
    .Name("GaniPay.TopUp.Worker.initiate")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wProvider = zeebe.NewWorker()
    .JobType("mock.topup.provider.charge")
    .Handler(mock.Handle)
    .Name("GaniPay.TopUp.Worker.provider")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wCredit = zeebe.NewWorker()
    .JobType("accounting.topup.credit")
    .Handler(credit.Handle)
    .Name("GaniPay.TopUp.Worker.credit")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wComplete = zeebe.NewWorker()
    .JobType("payments.topup.complete")
    .Handler(complete.Handle)
    .Name("GaniPay.TopUp.Worker.complete")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wNotify = zeebe.NewWorker()
    .JobType("mock.topup.notify.send")
    .Handler(mock.Handle)
    .Name("GaniPay.TopUp.Worker.notify")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();