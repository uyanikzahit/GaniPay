using GaniPay.DataCreation.Worker;
using GaniPay.DataCreation.Worker.Handlers;
using GaniPay.DataCreation.Worker.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

// Worker options
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

// Downstream: Identity (named client)
var identityBaseUrl = builder.Configuration["Downstream:Identity:BaseUrl"] ?? "http://localhost:5102";

builder.Services.AddHttpClient("identity", c =>
{
    c.BaseAddress = new Uri(identityBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ✅ Handlers + Host
builder.Services.AddSingleton<DataCreationJobHandlers>();
builder.Services.AddHostedService<WorkerHost>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();
await host.RunAsync();
