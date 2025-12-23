using GaniPay.Integration.Application.Contracts.Requests;
using GaniPay.Integration.Application.Services;
using GaniPay.Integration.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure + App services + DbContext
builder.Services.AddIntegrationInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseSwagger();
app.UseSwaggerUI();

var group = app.MapGroup("/api/integration").WithTags("Integration");

// 1) Provider call
group.MapPost("/call", async (IIntegrationService service, CallIntegrationRequest request, CancellationToken ct) =>
{
    var result = await service.CallAsync(request, ct);
    return Results.Ok(result);
});

// 2) Get log by id
group.MapGet("/logs/{id:guid}", async (IIntegrationService service, Guid id, CancellationToken ct) =>
{
    var result = await service.GetAsync(new GetIntegrationLogRequest(id), ct);
    return Results.Ok(result);
});

// 3) Get logs by provider id
group.MapGet("/providers/{providerId:guid}/logs", async (IIntegrationService service, Guid providerId, CancellationToken ct) =>
{
    var result = await service.GetProviderLogsAsync(new GetProviderLogsRequest(providerId), ct);
    return Results.Ok(result);
});

app.Run();
