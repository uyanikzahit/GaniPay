var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Microsoft.OpenApi.Models kullanmadan çalýþýr

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- In-memory demo store (MVP) ---
var customers = new Dictionary<Guid, CustomerDto>();

// --- System ---
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithTags("System");

// --- Customer Endpoints (Minimal API) ---

// POST /api/v1/customers  (Create Individual/Corporate)
app.MapPost("/api/v1/customers", (CreateCustomerRequest req) =>
{
    var now = DateTime.UtcNow;

    var id = Guid.NewGuid();
    var dto = new CustomerDto(
        Id: id,
        CustomerNumber: string.IsNullOrWhiteSpace(req.CustomerNumber)
            ? $"CUST-{now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}"
            : req.CustomerNumber!,
        Type: req.Type,
        Segment: req.Segment,
        Status: CustomerStatus.Active,
        OpenDate: req.OpenDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
        CloseDate: null,
        CloseReason: null,
        CreatedAt: now,
        UpdatedAt: now,
        Individual: req.Individual,
        Addresses: req.Addresses ?? new List<AddressDto>(),
        Phones: req.Phones ?? new List<PhoneDto>(),
        Emails: req.Emails ?? new List<EmailDto>()
    );

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

// PATCH /api/v1/customers/{id}  (basic update)
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


// ---------------- DTOs & Enums ----------------

enum CustomerType
{
    Individual = 1,
    Corporate = 2
}

enum CustomerSegment
{
    Basic = 1,
    FullOrPlatinum = 2
}

enum CustomerStatus
{
    Passive = 0,
    Active = 1,
    Blocked = 2,
    Closed = 3
}

record CustomerDto(
    Guid Id,
    string CustomerNumber,
    CustomerType Type,
    CustomerSegment Segment,
    CustomerStatus Status,
    DateOnly OpenDate,
    DateOnly? CloseDate,
    string? CloseReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IndividualCustomerDto? Individual,
    List<AddressDto> Addresses,
    List<PhoneDto> Phones,
    List<EmailDto> Emails
);

record CreateCustomerRequest(
    string? CustomerNumber,
    CustomerType Type,
    CustomerSegment Segment,
    DateOnly? OpenDate,
    IndividualCustomerDto? Individual,
    List<AddressDto>? Addresses,
    List<PhoneDto>? Phones,
    List<EmailDto>? Emails
);

record UpdateCustomerRequest(
    CustomerSegment? Segment,
    CustomerStatus? Status,
    DateOnly? CloseDate,
    string? CloseReason
);

// Individual detail (customer_individual)
record IndividualCustomerDto(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Nationality,
    string IdentityNumber
);

// customer_address
record AddressDto(
    string AddressType,
    string City,
    string District,
    string PostalCode,
    string AddressLine1
);

// customer_phone
record PhoneDto(
    string CountryCode,
    string PhoneNumber,
    int Type // MVP: int, sonra enum yaparýz (home/work/mobile)
);

// customer_email
record EmailDto(
    string EmailAddress,
    int Type, // MVP: int, sonra enum (personal/work)
    bool IsVerified
);
