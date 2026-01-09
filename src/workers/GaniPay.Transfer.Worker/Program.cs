using System;
using GaniPay.Transfer.Worker.Handlers;
using GaniPay.Transfer.Worker.Options;
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

// HttpClients (şimdilik dursun; real’lerde kullanacağız)
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

// Handlers (şimdilik sadece Mock)
builder.Services.AddSingleton<TransferMockJobHandler>();

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

// Resolve
var zeebe = host.Services.GetRequiredService<IZeebeClient>();
var mock = host.Services.GetRequiredService<TransferMockJobHandler>();

// MOCK Workers (TopUp pattern)
using var wResolveReceiver = zeebe.NewWorker()
    .JobType("transfer.receiver.resolve")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.resolveReceiver.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wExpense = zeebe.NewWorker()
    .JobType("transfer.expense.get")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.expense.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wInitiate = zeebe.NewWorker()
    .JobType("payments.transfer.initiate")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.initiate.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wNotifySender = zeebe.NewWorker()
    .JobType("notification.send.sender")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.notify.sender.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wNotifyReceiver = zeebe.NewWorker()
    .JobType("notification.send.receiver")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.notify.receiver.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var wComplete = zeebe.NewWorker()
    .JobType("payments.transfer.complete")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.complete.mock")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();
