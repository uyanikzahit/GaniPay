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

// Handlers (REAL)
builder.Services.AddSingleton<AccountingSenderAccountGetJobHandler>();
builder.Services.AddSingleton<AccountingReceiverAccountGetJobHandler>();
builder.Services.AddSingleton<AccountingAccountLedgerJobHandler>();

// Handlers (MOCK)
builder.Services.AddSingleton<TransferMockJobHandler>();

// Zeebe Client
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

var senderGet = host.Services.GetRequiredService<AccountingSenderAccountGetJobHandler>();
var receiverGet = host.Services.GetRequiredService<AccountingReceiverAccountGetJobHandler>();
var ledger = host.Services.GetRequiredService<AccountingAccountLedgerJobHandler>();
var mock = host.Services.GetRequiredService<TransferMockJobHandler>();

// Workers (using var + Timeout) ✅

// 1) Sender Account Get (REAL)
using var wSenderGet = zeebe.NewWorker()
    .JobType("accounting.sender.get")
    .Handler(senderGet.Handle)
    .Name("GaniPay.Transfer.Worker.senderGet")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 2) Receiver Account Get (REAL)
using var wReceiverGet = zeebe.NewWorker()
    .JobType("accounting.receiver.get")
    .Handler(receiverGet.Handle)
    .Name("GaniPay.Transfer.Worker.receiverGet")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 3) Resolve Receiver (AML) (MOCK)
using var wResolveAml = zeebe.NewWorker()
    .JobType("resolve.receiver.aml")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.resolveAml")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 4) Expense Get (MOCK)
using var wExpense = zeebe.NewWorker()
    .JobType("transfer.expense.get")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.expense")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 5) Initiate Transfer (Payments) (MOCK)
using var wInitiate = zeebe.NewWorker()
    .JobType("payments.transfer.initiate")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.initiate")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 6) Account Ledger (REAL)
using var wLedger = zeebe.NewWorker()
    .JobType("accounting.account.ledger")
    .Handler(ledger.Handle)
    .Name("GaniPay.Transfer.Worker.ledger")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 7) Receiver Balance Increase (MOCK)
using var wReceiverInc = zeebe.NewWorker()
    .JobType("receiver.balance.increase")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.receiverInc")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 8) Sender Balance Decrease (MOCK)
using var wSenderDec = zeebe.NewWorker()
    .JobType("sender.balance.decrease")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.senderDec")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 9) Complete Transfer (MOCK)
using var wComplete = zeebe.NewWorker()
    .JobType("payments.transfer.complete")
    .Handler(mock.Handle)
    .Name("GaniPay.Transfer.Worker.complete")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();
