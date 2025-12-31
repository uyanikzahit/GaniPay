using GaniPay.DataCreation.Worker;
using GaniPay.DataCreation.Worker.Handlers;
using GaniPay.DataCreation.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

// Downstream base urls
var customerBaseUrl = builder.Configuration["Downstream:Customer:BaseUrl"] ?? "https://localhost:7101";
var accountingBaseUrl = builder.Configuration["Downstream:Accounting:BaseUrl"] ?? "http://localhost:5103";
var identityBaseUrl = builder.Configuration["Downstream:Identity:BaseUrl"] ?? "http://localhost:5102";

// Named HttpClients
builder.Services.AddHttpClient("customer", c =>
{
    c.BaseAddress = new Uri(customerBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("accounting", c =>
{
    c.BaseAddress = new Uri(accountingBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("identity", c =>
{
    c.BaseAddress = new Uri(identityBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSingleton<DataCreationJobHandlers>();
builder.Services.AddHostedService<WorkerHost>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();
await host.RunAsync();