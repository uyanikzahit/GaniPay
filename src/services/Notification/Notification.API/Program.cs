using GaniPay.Notification.Application.Contracts.Requests;
using GaniPay.Notification.Application.Services;
using GaniPay.Notification.Infrastructure.DependencyInjection;
using GaniPay.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNotificationInfrastructure(builder.Configuration);

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ✅ Controlled auto-migration (Dev / Config-based)
// Development'ta otomatik çalışır. İstersen appsettings ile de aç/kapat yaparsın.
var autoMigrate =
    app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("Database:AutoMigrate");

if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

// Endpoints
var group = app.MapGroup("/api/notifications").WithTags("Notifications");

group.MapPost("/send", async (INotificationService service, SendNotificationRequest request, CancellationToken ct) =>
{
    var result = await service.SendAsync(request, ct);
    return Results.Ok(result);
});

group.MapGet("/{id:guid}", async (INotificationService service, Guid id, CancellationToken ct) =>
{
    var result = await service.GetAsync(new GetNotificationRequest(id), ct);
    return Results.Ok(result);
});

group.MapGet("/customers/{customerId:guid}", async (INotificationService service, Guid customerId, CancellationToken ct) =>
{
    var result = await service.GetCustomerLogsAsync(new GetCustomerNotificationsRequest(customerId), ct);
    return Results.Ok(result);
});

app.Run();
