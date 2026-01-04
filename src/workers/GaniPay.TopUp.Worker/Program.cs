using GaniPay.TopUp.Worker.Handlers;
using GaniPay.TopUp.Worker.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Zeebe.Client;

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
});

builder.Services.AddHttpClient("accounting", (sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<AccountingApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
});

// handlers
builder.Services.AddSingleton<TopUpValidateJobHandler>();

var host = builder.Build();

// Zeebe
var zeebeOpt = host.Services.GetRequiredService<IOptions<ZeebeOptions>>().Value;

// ⚠️ Aşağıdaki builder satırı da login worker’dakiyle birebir aynı olmalı.
var zeebe = ZeebeClient.Builder()
    .UseGatewayAddress(zeebeOpt.GatewayAddress)
    .UsePlainText()
    .Build();

var validate = host.Services.GetRequiredService<TopUpValidateJobHandler>();

var validateHandler = host.Services.GetRequiredService<TopUpValidateJobHandler>();

zeebe.NewWorker()
    .JobType("topup.validate")
    .Handler(async (client, job) => await validateHandler.Handle(client, job))
    .Name("topup.validate.worker")
    .MaxJobsActive(32)
    .Open();

await host.RunAsync();
