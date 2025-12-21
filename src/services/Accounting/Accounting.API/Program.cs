using GaniPay.Accounting.Application.Contracts.Requests;
using GaniPay.Accounting.Application.Services;
using GaniPay.Accounting.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAccountingInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health")
   .WithTags("Health");

var group = app.MapGroup("/api/accounting").WithTags("Accounting");

group.MapPost("/accounts", async (
        IAccountingService service,
        [FromBody] CreateAccountRequest request,
        CancellationToken ct) =>
{
    try
    {
        var created = await service.CreateAccountAsync(request, ct);
        return Results.Created($"/api/accounting/accounts/{created.Id}", created);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
});

group.MapGet("/customers/{customerId:guid}/balance", async (
        IAccountingService service,
        [FromRoute] Guid customerId,
        [FromQuery] string currency,
        CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(currency))
        return Results.BadRequest(new { message = "currency is required. Example: ?currency=TRY" });

    try
    {
        var balance = await service.GetBalanceAsync(customerId, currency, ct);
        return Results.Ok(balance);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
});

group.MapPost("/transactions/book", async (
        IAccountingService service,
        [FromBody] BookTransactionRequest request,
        CancellationToken ct) =>
{
    try
    {
        var booked = await service.BookTransactionAsync(request, ct);
        return Results.Ok(booked);
    }
    catch (InvalidOperationException ex)
    {
        return Results.UnprocessableEntity(new { message = ex.Message });
    }
});

group.MapPost("/usage", async (
        IAccountingService service,
        [FromBody] UsageQueryRequest request,
        CancellationToken ct) =>
{
    try
    {
        var usage = await service.GetUsageAsync(request, ct);
        return Results.Ok(usage);
    }
    catch (InvalidOperationException ex)
    {
        return Results.UnprocessableEntity(new { message = ex.Message });
    }
});

app.Run();
