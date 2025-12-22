using GaniPay.Payments.Application.Contracts.Requests;
using GaniPay.Payments.Application.Services;
using GaniPay.Payments.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext + Repo + Service DI burada
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var group = app.MapGroup("/api/payments").WithTags("Payments");

group.MapPost("/transfers", async (IPaymentsService service, StartTransferRequest request, CancellationToken ct) =>
{
    var result = await service.StartTransferAsync(request, ct);
    return Results.Ok(result);
});

group.MapPost("/topups", async (IPaymentsService service, StartTopUpRequest request, CancellationToken ct) =>
{
    var result = await service.StartTopUpAsync(request, ct);
    return Results.Ok(result);
});

group.MapGet("/{correlationId}", async (IPaymentsService service, string correlationId, CancellationToken ct) =>
{
    var result = await service.GetStatusAsync(new GetPaymentStatusRequest(correlationId), ct);
    return Results.Ok(result);
});

app.Run();
