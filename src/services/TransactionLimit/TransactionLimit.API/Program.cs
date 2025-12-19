using System.Text.Json.Serialization;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Application.Services;
using GaniPay.TransactionLimit.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON: enum string (genel standart)
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// DI (DbContext + repos + services)
builder.Services.AddTransactionLimitInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { service = "transaction-limit", status = "ok" }));

// ---- Limit Definitions ----
app.MapGet("/api/transaction-limit/limit-definitions", async (ITransactionLimitService svc, CancellationToken ct) =>
{
    var result = await svc.GetLimitDefinitionsAsync(ct);
    return Results.Ok(result);
});

app.MapPost("/api/transaction-limit/limit-definitions", async (CreateLimitDefinitionRequest req, ITransactionLimitService svc, CancellationToken ct) =>
{
    var result = await svc.CreateLimitDefinitionAsync(req, ct);
    return Results.Created($"/api/transaction-limit/limit-definitions/{result.Id}", result);
});

// ---- Customer Limits ----
app.MapGet("/api/transaction-limit/customers/{customerId:guid}/limits", async (Guid customerId, ITransactionLimitService svc, CancellationToken ct) =>
{
    var result = await svc.GetCustomerLimitsAsync(customerId, ct);
    return Results.Ok(result);
});

app.MapPost("/api/transaction-limit/customers/{customerId:guid}/limits",
async (Guid customerId, CreateCustomerLimitRequest req, ITransactionLimitService svc, CancellationToken ct) =>
{
    if (customerId != req.CustomerId)
        return Results.BadRequest(new { message = "customerId path/body mismatch" });

    var result = await svc.CreateCustomerLimitAsync(req, ct);

    // ✅ BURASI ÖNEMLİ: {customerId:guid} YOK!
    return Results.Created($"/api/transaction-limit/customers/{customerId}/limits/{result.Id}", result);
});

// ---- Check ----
app.MapPost("/api/transaction-limit/check", async (LimitCheckRequest req, ITransactionLimitService svc, CancellationToken ct) =>
{
    var result = await svc.CheckAsync(req, ct);
    return Results.Ok(result);
});

app.Run();
