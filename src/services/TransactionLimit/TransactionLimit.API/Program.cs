using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Application.Services;

using GaniPay.TransactionLimit.Infrastructure.Persistence;
using GaniPay.TransactionLimit.Infrastructure.Repositories;

using GaniPay.TransactionLimit.Application.Contracts.Enums;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// JSON
// -------------------------
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;

    // Enum string desteği (Day/Month/Year gibi)
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // Swagger’dan gelen "day", "amount" gibi değerleri daha toleranslı parse etmek için
    o.SerializerOptions.Converters.Add(new LimitPeriodJsonConverter());
    o.SerializerOptions.Converters.Add(new LimitMetricTypeJsonConverter());
    o.SerializerOptions.Converters.Add(new LimitSourceJsonConverter());
});

// -------------------------
// Swagger
// (OpenApi.Models / OpenApi.Any / SchemaFilter YOK)
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// DB + DI
// -------------------------
var connStr =
    builder.Configuration.GetConnectionString("TransactionLimitDb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:TransactionLimitDb"]
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("Connection string bulunamadı. appsettings.json -> ConnectionStrings:TransactionLimitDb ekle.");

builder.Services.AddDbContext<TransactionLimitDbContext>(opt => opt.UseNpgsql(connStr));

builder.Services.AddScoped<ILimitDefinitionRepository, LimitDefinitionRepository>();
builder.Services.AddScoped<ICustomerLimitRepository, CustomerLimitRepository>();
builder.Services.AddScoped<ITransactionLimitService, TransactionLimitService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health");

// -------------------------
// Routes
// -------------------------
var group = app.MapGroup("/api/transaction-limit");

// 1) GET /api/transaction-limit/limit-definitions
group.MapGet("/limit-definitions",
    async (ITransactionLimitService service, CancellationToken ct) =>
    {
        var items = await service.GetLimitDefinitionsAsync(ct);
        return Results.Ok(items);
    })
    .WithName("GetLimitDefinitions")
    .Produces(StatusCodes.Status200OK);

// 2) POST /api/transaction-limit/limit-definitions
group.MapPost("/limit-definitions",
    async (ITransactionLimitService service, CreateLimitDefinitionRequest request, CancellationToken ct) =>
    {
        var created = await service.CreateLimitDefinitionAsync(request, ct);
        return Results.Created($"/api/transaction-limit/limit-definitions/{created.Id}", created);
    })
    .WithName("CreateLimitDefinition")
    .Accepts<CreateLimitDefinitionRequest>("application/json")
    .Produces(StatusCodes.Status201Created);

// 3) GET /api/transaction-limit/customers/{customerId}/limits
group.MapGet("/customers/{customerId:guid}/limits",
    async (ITransactionLimitService service, Guid customerId, CancellationToken ct) =>
    {
        var items = await service.GetCustomerLimitsAsync(customerId, ct);
        return Results.Ok(items);
    })
    .WithName("GetCustomerLimits")
    .Produces(StatusCodes.Status200OK);

// 4) POST /api/transaction-limit/customers/{customerId}/limits
// HATA SENDEN BURADA GELİYORDU: CreateCustomerLimitAsync(customerId, request, ct) olmalı.
group.MapPost("/customers/{customerId:guid}/limits",
    async (ITransactionLimitService service, Guid customerId, CreateCustomerLimitRequest request, CancellationToken ct) =>
    {
        var created = await service.CreateCustomerLimitAsync(customerId, request, ct);
        return Results.Created($"/api/transaction-limit/customers/{customerId}/limits/{created.Id}", created);
    })
    .WithName("CreateCustomerLimit")
    .Accepts<CreateCustomerLimitRequest>("application/json")
    .Produces(StatusCodes.Status201Created);

// 5) POST /api/transaction-limit/check
group.MapPost("/check",
    async (ITransactionLimitService service, LimitCheckRequest request, CancellationToken ct) =>
    {
        var result = await service.CheckAsync(request, ct);
        return Results.Ok(result);
    })
    .WithName("CheckLimit")
    .Accepts<LimitCheckRequest>("application/json")
    .Produces(StatusCodes.Status200OK);

app.Run();


// =====================================================
// JSON Converters (toleranslı enum okuma)
// =====================================================

sealed class LimitPeriodJsonConverter : JsonConverter<LimitPeriod>
{
    public override LimitPeriod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var n))
            return (LimitPeriod)n;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("LimitPeriod JSON string olmalı.");

        var s = (reader.GetString() ?? "").Trim();

        if (Enum.TryParse<LimitPeriod>(s, true, out var parsed))
            return parsed;

        var lower = s.ToLowerInvariant();
        var candidates = lower switch
        {
            "day" or "daily" => new[] { "Day", "Daily" },
            "month" or "monthly" => new[] { "Month", "Monthly" },
            "year" or "yearly" or "annual" => new[] { "Year", "Yearly", "Annual" },
            _ => Array.Empty<string>()
        };

        foreach (var c in candidates)
            if (Enum.TryParse<LimitPeriod>(c, true, out parsed))
                return parsed;

        throw new JsonException($"LimitPeriod parse edilemedi: '{s}'.");
    }

    public override void Write(Utf8JsonWriter writer, LimitPeriod value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

sealed class LimitMetricTypeJsonConverter : JsonConverter<LimitMetricType>
{
    public override LimitMetricType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var n))
            return (LimitMetricType)n;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("LimitMetricType JSON string olmalı.");

        var s = (reader.GetString() ?? "").Trim();

        if (Enum.TryParse<LimitMetricType>(s, true, out var parsed))
            return parsed;

        var lower = s.ToLowerInvariant();
        var candidates = lower switch
        {
            "amount" or "transferamount" or "money" => new[] { "Amount", "TransferAmount" },
            "count" or "transactioncount" => new[] { "Count", "TransactionCount" },
            "balance" => new[] { "Balance" },
            _ => Array.Empty<string>()
        };

        foreach (var c in candidates)
            if (Enum.TryParse<LimitMetricType>(c, true, out parsed))
                return parsed;

        throw new JsonException($"LimitMetricType parse edilemedi: '{s}'.");
    }

    public override void Write(Utf8JsonWriter writer, LimitMetricType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

sealed class LimitSourceJsonConverter : JsonConverter<LimitSource>
{
    public override LimitSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var n))
            return (LimitSource)n;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("LimitSource JSON string olmalı.");

        var s = (reader.GetString() ?? "").Trim();

        if (Enum.TryParse<LimitSource>(s, true, out var parsed))
            return parsed;

        var lower = s.ToLowerInvariant();
        var candidates = lower switch
        {
            "system" => new[] { "System" },
            "migration" => new[] { "Migration" },
            "admin" => new[] { "Admin" },
            _ => Array.Empty<string>()
        };

        foreach (var c in candidates)
            if (Enum.TryParse<LimitSource>(c, true, out parsed))
                return parsed;

        throw new JsonException($"LimitSource parse edilemedi: '{s}'.");
    }

    public override void Write(Utf8JsonWriter writer, LimitSource value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
