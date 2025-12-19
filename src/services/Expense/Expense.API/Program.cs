using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Application.Requests;
using GaniPay.Expense.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExpenseInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("ok"));

var group = app.MapGroup("/api/v1/expenses");

// ExpenseDefinition CRUD
group.MapGet("/", async (IExpenseService service, CancellationToken ct) =>
{
    var list = await service.ListAsync(ct);
    return Results.Ok(list);
});

group.MapGet("/{id:guid}", async (Guid id, IExpenseService service, CancellationToken ct) =>
{
    var dto = await service.GetByIdAsync(id, ct);
    return Results.Ok(dto);
});

group.MapGet("/code/{code}", async (string code, IExpenseService service, CancellationToken ct) =>
{
    var dto = await service.GetByCodeAsync(code, ct);
    return Results.Ok(dto);
});

group.MapPost("/", async (CreateExpenseRequest request, IExpenseService service, CancellationToken ct) =>
{
    var dto = await service.CreateAsync(request, ct);
    return Results.Created($"/api/v1/expenses/{dto.Id}", dto);
});

group.MapPut("/{id:guid}", async (Guid id, UpdateExpenseRequest request, IExpenseService service, CancellationToken ct) =>
{
    var dto = await service.UpdateAsync(id, request, ct);
    return Results.Ok(dto);
});

group.MapDelete("/{id:guid}", async (Guid id, IExpenseService service, CancellationToken ct) =>
{
    await service.DeleteAsync(id, ct);
    return Results.NoContent();
});

// Pending create (fee calculation result)
group.MapPost("/pending", async (CreateExpensePendingRequest request, IExpenseService service, CancellationToken ct) =>
{
    var dto = await service.CreatePendingAsync(request, ct);
    return Results.Created($"/api/v1/expenses/pending/{dto.Id}", dto);
});

app.Run();
