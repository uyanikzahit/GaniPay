using GaniPay.Customer.Application.Contracts;
using GaniPay.Customer.Application.Contracts.Dtos;
using GaniPay.Customer.Application.Contracts.Enums;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithTags("System");

// In-memory demo store (MVP)
var customers = new Dictionary<Guid, CustomerDto>();

// POST /api/v1/customers
app.MapPost("/api/v1/customers", (CreateCustomerRequest req) =>
{
    var now = DateTime.UtcNow;
    var id = Guid.NewGuid();

    var customerNumber = string.IsNullOrWhiteSpace(req.CustomerNumber)
        ? $"CUST-{now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}"
        : req.CustomerNumber;

    var dto = new CustomerDto
    {
        Id = id,
        CustomerNumber = customerNumber,
        Type = req.Type,
        Segment = req.Segment,
        Status = CustomerStatus.Active,
        OpenDate = req.OpenDate,
        CloseDate = null,
        CloseReason = null,
        CreatedAt = now,
        UpdatedAt = now,
        Individual = req.Individual,
        Addresses = req.Addresses ?? [],
        Phones = req.Phones ?? [],
        Emails = req.Emails ?? []
    };

    customers[id] = dto;
    return Results.Created($"/api/v1/customers/{id}", dto);
})
.WithTags("Customer");

// GET /api/v1/customers/{id}
app.MapGet("/api/v1/customers/{id:guid}", (Guid id) =>
{
    return customers.TryGetValue(id, out var dto)
        ? Results.Ok(dto)
        : Results.NotFound(new { message = "Customer not found", id });
})
.WithTags("Customer");

// PATCH /api/v1/customers/{id}
app.MapPatch("/api/v1/customers/{id:guid}", (Guid id, UpdateCustomerRequest req) =>
{
    if (!customers.TryGetValue(id, out var existing))
        return Results.NotFound(new { message = "Customer not found", id });

    var updated = existing with
    {
        Segment = req.Segment ?? existing.Segment,
        Status = req.Status ?? existing.Status,
        CloseDate = req.CloseDate ?? existing.CloseDate,
        CloseReason = req.CloseReason ?? existing.CloseReason,
        UpdatedAt = DateTime.UtcNow
    };

    customers[id] = updated;
    return Results.Ok(updated);
})
.WithTags("Customer");

app.Run();
