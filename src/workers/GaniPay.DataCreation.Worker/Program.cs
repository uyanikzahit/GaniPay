using GaniPay.DataCreation.Worker;
using GaniPay.DataCreation.Worker.Handlers;
using GaniPay.DataCreation.Worker.Options;
using GaniPay.DataCreation.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

builder.Services.AddHttpClient<ApiClients>();

builder.Services.AddSingleton<DataCreationJobHandlers>();
builder.Services.AddHostedService<WorkerHost>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();
await host.RunAsync();
