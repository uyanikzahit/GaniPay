using System.Text.Json.Serialization;
using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Domain.Entities;
using GaniPay.TransactionLimit.Domain.Enums;
using GaniPay.TransactionLimit.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON: enum string
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// DI
builder.Services.AddTransactionLimitInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { service = "transaction-limit", status = "ok" }));

// -----------------------------
// Limit Definitions (MVP CRUD-lite)
// -----------------------------
app.MapGet("/api/transaction-limit/limit-definitions", async (ILimitDefinitionRepository repo, CancellationToken ct) =>
{
    var list = await repo.ListAsync(ct);
    return Results.Ok(list);
});

app.MapPost("/api/transaction-limit/limit-definitions", async (CreateLimitDefinitionRequest req, ILimitDefinitionRepository repo, CancellationToken ct) =>
{
    // string -> enum parse (case-insensitive)
    if (!Enum.TryParse<LimitPeriod>(req.Period, true, out var period))
        return Results.BadRequest(new { message = "period invalid (day/month/year)" });

    if (!Enum.TryParse<LimitMetricType>(req.MetricType, true, out var metricType))
        return Results.BadRequest(new { message = "metricType invalid (amount/count/balance)" });

    var exists = await repo.GetByCodeAsync(req.Code, ct);
    if (exists is not null)
        return Results.Conflict(new { message = "code already exists" });

    var entity = new LimitDefinition
    {
        Id = Guid.NewGuid(),
        Code = req.Code,
        Name = req.Name,
        Description = req.Description,
        Period = period,
        MetricType = metricType,
        IsVisible = req.IsVisible
    };

    await repo.AddAsync(entity, ct);
    return Results.Created($"/api/transaction-limit/limit-definitions/{entity.Id}", entity);
});

// -----------------------------
// Customer Limits (assign/list)
// -----------------------------
app.MapGet("/api/transaction-limit/customers/{customerId:guid}/limits", async (Guid customerId, ICustomerLimitRepository repo, CancellationToken ct) =>
{
    var list = await repo.ListByCustomerIdAsync(customerId, ct);
    return Results.Ok(list);
});

app.MapPost("/api/transaction-limit/customers/{customerId:guid}/limits", async (
    Guid customerId,
    CreateCustomerLimitRequest req,
    ICustomerLimitRepository repo,
    ILimitDefinitionRepository defRepo,
    CancellationToken ct) =>
{
    if (customerId != req.CustomerId)
        return Results.BadRequest(new { message = "customerId path/body mismatch" });

    var def = await defRepo.GetByIdAsync(req.LimitDefinitionId, ct);
    if (def is null)
        return Results.NotFound(new { message = "limit definition not found" });

    if (!Enum.TryParse<LimitSource>(req.Source, true, out var source))
        return Results.BadRequest(new { message = "source invalid (system/admin/migration)" });

    var entity = new CustomerLimit
    {
        Id = Guid.NewGuid(),
        CustomerId = req.CustomerId,
        LimitDefinitionId = req.LimitDefinitionId,
        Year = req.Year,
        Month = req.Month,
        Day = req.Day,
        Value = req.Value,
        Currency = req.Currency,
        Source = source,
        Reason = req.Reason,
        CreatedAt = DateTime.UtcNow
    };

    await repo.AddAsync(entity, ct);
    return Results.Created($"/api/transaction-limit/customers/{customerId:guid}/limits/{entity.Id}", entity);
});

// -----------------------------
// Check (MVP): limit var mı / yeterli mi?
// Not: consumption (used_value) MVP’de yok; sadece "var mı" kontrol.
// -----------------------------
app.MapPost("/api/transaction-limit/check", async (
    Guid customerId,
    string code,
    decimal amount,
    ILimitDefinitionRepository defRepo,
    ICustomerLimitRepository custRepo,
    CancellationToken ct) =>
{
    var def = await defRepo.GetByCodeAsync(code, ct);
    if (def is null)
        return Results.NotFound(new { message = "limit definition not found" });

    var limits = await custRepo.ListByCustomerIdAsync(customerId, ct);
    var match = limits.FirstOrDefault(x => x.LimitDefinitionId == def.Id);

    if (match is null)
        return Results.Ok(new { allowed = true, reason = "no customer-specific limit (MVP default allow)" });

    // MetricType=Amount için amount kıyas (MVP)
    if (def.MetricType == LimitMetricType.Amount && amount > match.Value)
        return Results.Ok(new { allowed = false, reason = "limit exceeded", limit = match.Value, requested = amount });

    return Results.Ok(new { allowed = true, reason = "ok", limit = match.Value, requested = amount });
});

app.Run();
