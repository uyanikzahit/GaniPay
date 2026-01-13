using GaniPay.Login.Worker.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zeebe.Client;

var builder = Host.CreateApplicationBuilder(args);

// Options
builder.Services.Configure<ZeebeOptions>(builder.Configuration.GetSection("Zeebe"));
builder.Services.Configure<IdentityApiOptions>(builder.Configuration.GetSection("IdentityApi"));
builder.Services.Configure<WorkflowApiOptions>(builder.Configuration.GetSection("WorkflowApi"));

// HttpClient (DEV: https local sertifika için ignore)
builder.Services.AddHttpClient<AuthFlowJobHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Handlers
builder.Services.AddSingleton<IdentityLoginJobHandler>(); // REAL
// ❗ AuthFlowJobHandler burada AddSingleton OLMAYACAK (HttpClient ile geliyor)

// Zeebe Client
builder.Services.AddSingleton<IZeebeClient>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<ZeebeOptions>>().Value;

    return ZeebeClient.Builder()
        .UseGatewayAddress(opt.GatewayAddress)
        .UsePlainText() // local dev
        .Build();
});

var host = builder.Build();

var zeebe = host.Services.GetRequiredService<IZeebeClient>();
var identityHandler = host.Services.GetRequiredService<IdentityLoginJobHandler>();
var flowHandler = host.Services.GetRequiredService<AuthFlowJobHandler>();

// 1) REAL
using var identityWorker = zeebe.NewWorker()
    .JobType("identity.login")
    .Handler(identityHandler.Handle)
    .Name("GaniPay.Login.Worker.identity")
    .MaxJobsActive(10)
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

// 2) MOCK job types (tek handler class)
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

using var accountWorker = zeebe.NewWorker()
    .JobType("mock.auth.account.status")
    .Handler(flowHandler.Handle)
    .Name("GaniPay.Login.Worker.account")
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


// ---- Options Classes (aynı dosyada kalsın) ----
public sealed class ZeebeOptions
{
    public string GatewayAddress { get; set; } = "127.0.0.1:26500";
}

public sealed class IdentityApiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5102";
}

public sealed class WorkflowApiOptions
{
    public string BaseUrl { get; set; } = "https://localhost:7253";
}
