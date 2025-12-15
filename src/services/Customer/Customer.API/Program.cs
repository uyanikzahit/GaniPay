using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GaniPay Customer API",
        Version = "v1",
        Description = "Customer domain API"
    });

    // (Bazý projelerde þema isim çakýþmasýný engeller)
    c.CustomSchemaIds(t => t.FullName);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 500'ün gerçek sebebini ekranda görmek için
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GaniPay Customer API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "customer-api" }))
   .WithTags("System");

var api = app.MapGroup("/api/v1").WithTags("Customer");

api.MapGet("/customers/{id:guid}", (Guid id) =>
    Results.Ok(new { id, name = "stub" })
).WithName("GetCustomer");

api.MapPost("/customers", (CreateCustomerRequest req) =>
{
    var id = Guid.NewGuid();
    return Results.Created($"/api/v1/customers/{id}", new { id, req.Name, req.Email });
}).WithName("CreateCustomer");

api.MapPatch("/customers/{id:guid}", (Guid id, UpdateCustomerRequest req) =>
    Results.NoContent()
).WithName("UpdateCustomer");

app.Run();

record CreateCustomerRequest(string Name, string Email);
record UpdateCustomerRequest(string? Name, string? Email);
