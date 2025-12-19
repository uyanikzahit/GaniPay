using GaniPay.TransactionLimit.Application.Services;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger (OpenApiInfo kullanmadan)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Aynı isimli DTO/Request çakışmalarında patlamasın diye
    c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
});

// Infrastructure (DbContext + Repos)
builder.Services.AddTransactionLimitInfrastructure(builder.Configuration);

// Application Service
builder.Services.AddScoped<ITransactionLimitService, TransactionLimitService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var group = app.MapGroup("/api/transaction-limit")
               .WithTags("TransactionLimit");

// 1) GET /api/transaction-limit/limit-definitions
group.MapGet("/limit-definitions",
    async (ITransactionLimitService service, CancellationToken ct) =>
    {
        var list = await service.GetLimitDefinitionsAsync(ct);
        return Results.Ok(list);
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
    async (Guid customerId, ITransactionLimitService service, CancellationToken ct) =>
    {
        var list = await service.GetCustomerLimitsAsync(customerId, ct);
        return Results.Ok(list);
    })
    .WithName("GetCustomerLimits")
    .Produces(StatusCodes.Status200OK);

// 4) POST /api/transaction-limit/customers/{customerId}/limits
group.MapPost("/customers/{customerId:guid}/limits",
    async (Guid customerId, ITransactionLimitService service, CreateCustomerLimitRequest request, CancellationToken ct) =>
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
