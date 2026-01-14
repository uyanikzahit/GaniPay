using GaniPay.Login.Worker.Handlers;
using GaniPay.Login.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zeebe.Client;

var builder = Host.CreateApplicationBuilder(args);

// -------------------- Options --------------------
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection("Zeebe"));
builder.Services.Configure<IdentityApiOptions>(builder.Configuration.GetSection("IdentityApi"));
builder.Services.Configure<CustomerApiOptions>(builder.Configuration.GetSection("CustomerApi"));
builder.Services.Configure<AccountingApiOptions>(builder.Configuration.GetSection("AccountingApi"));

// -------------------- HttpClient (DEV: https local sertifika için ignore) --------------------
// Typed clients: handler'lar HttpClient alacak şekilde yazılırsa en temiz yol bu.
builder.Services.AddHttpClient<AuthFlowJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(15));

builder.Services.AddHttpClient<IdentityLoginJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(15));

builder.Services.AddHttpClient<CustomerGetJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(15));

builder.Services.AddHttpClient<AccountGetJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(15));

// -------------------- Handlers --------------------
// REAL
builder.Services.AddSingleton<IdentityLoginJobHandler>();
builder.Services.AddSingleton<CustomerGetJobHandler>();
builder.Services.AddSingleton<AccountGetJobHandler>();

// MOCK (Typed HttpClient ile gelir -> AddSingleton verme)
// AuthFlowJobHandler typed client ile otomatik çözülür

// -------------------- Zeebe Client --------------------
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

var identityHandler = host.Services.GetRequiredService<IdentityLoginJobHandler>();
var customerHandler = host.Services.GetRequiredService<CustomerGetJobHandler>();
var accountHandler = host.Services.GetRequiredService<AccountGetJobHandler>();

var flowHandler = host.Services.GetRequiredService<AuthFlowJobHandler>();

// -------------------- WORKERS --------------------

// 1) REAL: identity.login
using var identityWorker = zeebe.NewWorker()
    .JobType("identity.login")
    .Handler(identityHandler.Handle)
    .Name("GaniPay.Login.Worker.identity")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 2) REAL: login.customer.get
using var customerWorker = zeebe.NewWorker()
    .JobType("login.customer.get")
    .Handler(customerHandler.Handle)
    .Name("GaniPay.Login.Worker.customer")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 3) REAL: login.account.get
using var accountWorker = zeebe.NewWorker()
    .JobType("login.account.get")
    .Handler(accountHandler.Handle)
    .Name("GaniPay.Login.Worker.account")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 4) MOCK job types (tek handler class)
using var prepareWorker = zeebe.NewWorker()
    .JobType("mock.auth.context.prepare")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.context")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var guardWorker = zeebe.NewWorker()
    .JobType("mock.auth.bruteforce.guard")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.guard")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var deviceWorker = zeebe.NewWorker()
    .JobType("mock.auth.device.trust")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.device")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var sessionWorker = zeebe.NewWorker()
    .JobType("mock.auth.session.create")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.session")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

using var auditWorker = zeebe.NewWorker()
    .JobType("mock.auth.audit.log")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.audit")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();
