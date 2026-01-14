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

// -------------------- HttpClient (DEV: sertifika ignore) --------------------
// Identity (http ise sorun yok ama aynı standartta kalsın)
builder.Services.AddHttpClient();

// Customer API (https olduğun için sertifika ignore şart)
builder.Services.AddHttpClient("customer")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Accounting API (http)
builder.Services.AddHttpClient("accounting");

// AuthFlowJobHandler (mock tasklar için) -> https ignore lazım olabilir diye aynı handler veriyoruz
builder.Services.AddHttpClient<AuthFlowJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// -------------------- Handlers --------------------
builder.Services.AddSingleton<IdentityLoginJobHandler>();     // REAL
builder.Services.AddSingleton<CustomerGetJobHandler>();       // REAL
builder.Services.AddSingleton<AccountGetJobHandler>();        // REAL
// AuthFlowJobHandler AddSingleton OLMAYACAK (typed HttpClient ile geliyor)

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

// 1) REAL - Identity Login
using var identityWorker = zeebe.NewWorker()
    .JobType("identity.login")
    .Handler(identityHandler.Handle)
    .Name("GaniPay.Login.Worker.identity")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 2) REAL - Customer Get
using var customerWorker = zeebe.NewWorker()
    .JobType("login.customer.get")
    .Handler(customerHandler.Handle)
    .Name("GaniPay.Login.Worker.customer")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 3) REAL - Account Get (account.active yerine geçti)
using var accountWorker = zeebe.NewWorker()
    .JobType("login.account.get")
    .Handler(accountHandler.Handle)
    .Name("GaniPay.Login.Worker.account")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 4) MOCK - Prepare Context
using var prepareWorker = zeebe.NewWorker()
    .JobType("mock.auth.context.prepare")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.context")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 5) MOCK - Bruteforce Guard
using var guardWorker = zeebe.NewWorker()
    .JobType("mock.auth.bruteforce.guard")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.guard")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 6) MOCK - Device Trust
using var deviceWorker = zeebe.NewWorker()
    .JobType("mock.auth.device.trust")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.device")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 7) MOCK - Session Create
using var sessionWorker = zeebe.NewWorker()
    .JobType("mock.auth.session.create")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.session")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 8) MOCK - Audit Log
using var auditWorker = zeebe.NewWorker()
    .JobType("mock.auth.audit.log")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.audit")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

await host.RunAsync();
