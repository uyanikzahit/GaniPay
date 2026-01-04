// Program.cs  (GaniPay.TopUp.Worker)

using System.Net.Http.Headers;
using GaniPay.TopUp.Worker.Handlers;
using GaniPay.TopUp.Worker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using zb_client;

var builder = Host.CreateApplicationBuilder(args);

// config
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// options
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection("Zeebe"));
builder.Services.Configure<PaymentsApiOptions>(builder.Configuration.GetSection("PaymentsApi"));
builder.Services.Configure<AccountingApiOptions>(builder.Configuration.GetSection("AccountingApi"));

// http clients
builder.Services.AddHttpClient("payments", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<PaymentsApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("accounting", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<AccountingApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// handlers
builder.Services.AddSingleton<TopUpValidateJobHandler>();
builder.Services.AddSingleton<AccountStatusJobHandler>();
builder.Services.AddSingleton<TopUpMockJobHandler>();
builder.Services.AddSingleton<PaymentsInitiateTopUpJobHandler>();
builder.Services.AddSingleton<AccountingCreditLedgerJobHandler>();
builder.Services.AddSingleton<PaymentsCompleteTopUpJobHandler>();
builder.Services.AddSingleton<PaymentsStatusUpdateJobHandler>();

var host = builder.Build();

// zeebe
var zeebeOpt = host.Services.GetRequiredService<IOptions<ZeebeOptions>>().Value;

using var zeebe = new ZbClientBuilder()
    .UseGatewayAddress(zeebeOpt.GatewayAddress)
    .UsePlainText()
    .Build();

// ---- Worker registrations (jobType'lar BPMN ile birebir) ----

// 1) Validate Request (REAL)
{
    var handler = host.Services.GetRequiredService<TopUpValidateJobHandler>();
    zeebe.NewWorker()
        .JobType("topup.validate")
        .Handler(async (c, j) => await handler.HandleJob(c, j))
        .Name("topup.validate.worker")
        .MaxJobsActive(32)
        .Open();
}

// 2) Account Status Check (REAL -> Accounting API GET /api/accounting/accounts/status?accountId=...&customerId=...&currency=...)
{
    var handler = host.Services.GetRequiredService<AccountStatusJobHandler>();
    zeebe.NewWorker()
        .JobType("accounting.account.status.check")
        .Handler(async (c, j) => await handler.HandleJob(c, j))
        .Name("account.status.worker")
        .MaxJobsActive(32)
        .Open();
}

// 3) Transaction Limit Check (MOCK)
{
    var handler = host.Services.GetRequiredService<TopUpMockJobHandler>();
    zeebe.NewWorker()
        .JobType("topup.limit.check")
        .Handler(async (c, j) => await handler.HandleLimitCheck(c, j))
        .Name("topup.limit.mock.worker")
        .MaxJobsActive(32)
        .Open();
}

// 4) Initiate TopUp (REAL -> Payments API POST /api/payments/topups)
{
    var handler = host.Services.GetRequiredService<PaymentsInitiateTopUpJobHandler>();
    zeebe.NewWorker()
        .JobType("payments.topup.initiate")
        .Handler(async (c, j) => await handler.HandleJob(c, j))
        .Name("payments.topup.initiate.worker")
        .MaxJobsActive(32)
        .Open();
}

// 5) Provider Charge (MOCK)
{
    var handler = host.Services.GetRequiredService<TopUpMockJobHandler>();
    zeebe.NewWorker()
        .JobType("mock.topup.provider.charge")
        .Handler(async (c, j) => await handler.HandleProviderCharge(c, j))
        .Name("topup.provider.mock.worker")
        .MaxJobsActive(32)
        .Open();
}

// 6) Credit Ledger (REAL -> Accounting API POST /api/accounting/transactions)
{
    var handler = host.Services.GetRequiredService<AccountingCreditLedgerJobHandler>();
    zeebe.NewWorker()
        .JobType("accounting.topup.credit")
        .Handler(async (c, j) => await handler.HandleJob(c, j))
        .Name("accounting.topup.credit.worker")
        .MaxJobsActive(32)
        .Open();
}

// 7) Complete TopUp (REAL -> Payments API POST /api/payments/status  (Succeeded/Failed))
{
    var handler = host.Services.GetRequiredService<PaymentsCompleteTopUpJobHandler>();
    zeebe.NewWorker()
        .JobType("payments.topup.complete")
        .Handler(async (c, j) => await handler.HandleJob(c, j))
        .Name("payments.topup.complete.worker")
        .MaxJobsActive(32)
        .Open();
}

// 8) Send Notification (MOCK)
{
    var handler = host.Services.GetRequiredService<TopUpMockJobHandler>();
    zeebe.NewWorker()
        .JobType("mock.topup.notify.send")
        .Handler(async (c, j) => await handler.HandleNotify(c, j))
        .Name("topup.notify.mock.worker")
        .MaxJobsActive(32)
        .Open();
}

// (İstersen ayrıca bağımsız status update jobType da bağlayabilirsin; BPMN’de kullanıyorsan aç)
// {
///    var handler = host.Services.GetRequiredService<PaymentsStatusUpdateJobHandler>();
//     zeebe.NewWorker()
//         .JobType("payments.status.update")
//         .Handler(async (c, j) => await handler.HandleJob(c, j))
//         .Name("payments.status.update.worker")
//         .MaxJobsActive(32)
//         .Open();
// }

await host.RunAsync();