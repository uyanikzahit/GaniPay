using GaniPay.Customer.Application.Services;
using GaniPay.Customer.Infrastructure.DependencyInjection;
using GaniPay.Customer.Application.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DbContext + Repos)
builder.Services.AddCustomerInfrastructure(builder.Configuration);

// Application Service
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "customer" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/v1/customers/individual", async (
    ICustomerService service,
    CreateIndividualCustomerRequest request,
    CancellationToken ct) =>
{
    var result = await service.CreateIndividualAsync(request, ct);
    return Results.Created($"/api/v1/customers/{result.Id}", result);
});

app.MapGet("/api/v1/customers/{customerId:guid}", async (
    ICustomerService service,
    Guid customerId,
    CancellationToken ct) =>
{
    var result = await service.GetByIdAsync(customerId, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/v1/customers/{customerId:guid}/emails", async (
    ICustomerService service,
    Guid customerId,
    AddEmailRequest request,
    CancellationToken ct) =>
{
    await service.AddEmailAsync(customerId, request, ct);
    return Results.NoContent();
});

app.MapPost("/api/v1/customers/{customerId:guid}/phones", async (
    ICustomerService service,
    Guid customerId,
    AddPhoneRequest request,
    CancellationToken ct) =>
{
    await service.AddPhoneAsync(customerId, request, ct);
    return Results.NoContent();
});

app.MapPost("/api/v1/customers/{customerId:guid}/addresses", async (
    ICustomerService service,
    Guid customerId,
    AddAddressRequest request,
    CancellationToken ct) =>
{
    await service.AddAddressAsync(customerId, request, ct);
    return Results.NoContent();
});

app.MapPost("/api/v1/customers/{customerId:guid}/close", async (
    ICustomerService service,
    Guid customerId,
    CloseCustomerRequest request,
    CancellationToken ct) =>
{
    await service.CloseAsync(customerId, request, ct);
    return Results.NoContent();
});

app.Run();
