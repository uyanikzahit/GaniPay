using GaniPay.Accounting.Application.Contracts.Requests;
using GaniPay.Accounting.Application.Services;
using GaniPay.Accounting.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAccountingInfrastructure(builder.Configuration);

// ✅ CORS (DEV)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseSwagger();
app.UseSwaggerUI();

// ✅ CORS middleware endpointlerden ÖNCE olmalı
app.UseCors("DevCors");

var group = app.MapGroup("/api/accounting").WithTags("Accounting");

// Create account
group.MapPost("/accounts", async (
    IAccountingService service,
    CreateAccountRequest request,
    CancellationToken ct) =>
{
    var created = await service.CreateAccountAsync(request, ct);
    return Results.Created($"/api/accounting/accounts/{created.Id}", created);
});

// Get balance by customer + currency
group.MapGet("/customers/{customerId:guid}/balance", async (
    IAccountingService service,
    Guid customerId,
    string currency,
    CancellationToken ct) =>
{
    var balance = await service.GetBalanceAsync(customerId, currency, ct);
    return Results.Ok(balance);
});

// Post transaction (book into accounting_transaction + history + update account.balance)
group.MapPost("/transactions", async (
    IAccountingService service,
    PostAccountingTransactionRequest request,
    CancellationToken ct) =>
{
    var tx = await service.PostTransactionAsync(request, ct);
    return Results.Ok(tx);
});

// Usage (transaction-limit usedValue buradan alacak)
group.MapPost("/usage", async (
    IAccountingService service,
    UsageQueryRequest request,
    CancellationToken ct) =>
{
    var usage = await service.GetUsageAsync(request, ct);
    return Results.Ok(usage);
});

group.MapGet("/customers/{customerId}/wallets", async (
    IAccountingService service,
    Guid customerId,
    CancellationToken ct) =>
{
    var result = await service.GetCustomerWalletsAsync(customerId, ct);
    return Results.Ok(result);
})
.WithName("GetCustomerWallets");

group.MapGet("/accounts/status", async (
    IAccountingService service,
    Guid accountId,
    Guid customerId,
    string currency,
    CancellationToken ct) =>
{
    var result = await service.GetAccountStatusAsync(customerId, currency, ct);
    return Results.Ok(result);
})
.WithName("GetAccountStatus");

// Get balance history by accountId
group.MapGet("/accounts/{accountId:guid}/balance-history", async (
    IAccountingService service,
    Guid accountId,
    CancellationToken ct) =>
{
    var result = await service.GetBalanceHistoryAsync(accountId, ct);
    return Results.Ok(result);
})
.WithName("GetAccountBalanceHistory");

app.Run();
