using GaniPay.Identity.Application.Services;
using GaniPay.Identity.Domain.Enums;
using GaniPay.Identity.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var group = app.MapGroup("/api/v1/identity").WithTags("Identity");

// POST /api/v1/identity/registrations/start
group.MapPost("/registrations/start", async (StartRegistrationBody body, IdentityService service, CancellationToken ct) =>
{
    var created = await service.StartRegistrationAsync(body.CustomerId, body.PhoneNumber, body.Password, ct);

    return Results.Created($"/api/v1/identity/credentials/{created.Id}", new
    {
        id = created.Id,
        customerId = created.CustomerId,
        loginType = created.LoginType,
        loginValue = created.LoginValue,
        failedLoginCount = created.FailedLoginCount,
        isLocked = created.IsLocked,
        lockReason = created.LockReason,
        lastLoginAt = created.LastLoginAt,
        createdAt = created.CreatedAt,
        updatedAt = created.UpdatedAt
    });
});

// POST /api/v1/identity/login
group.MapPost("/login", async (LoginBody body, IdentityService service, CancellationToken ct) =>
{
    var cred = await service.LoginAsync(body.PhoneNumber, body.Password, ct);

    return Results.Ok(new
    {
        id = cred.Id,
        customerId = cred.CustomerId,
        loginType = cred.LoginType,
        loginValue = cred.LoginValue,
        lastLoginAt = cred.LastLoginAt
    });
});

// POST /api/v1/identity/recovery/start
group.MapPost("/recovery/start", async (StartRecoveryBody body, IdentityService service, CancellationToken ct) =>
{
    var channel = Enum.TryParse<RecoveryChannel>(body.Channel, true, out var ch) ? ch : RecoveryChannel.Sms;
    var (recovery, plainToken) = await service.StartRecoveryAsync(body.PhoneNumber, channel, ct);

    // MVP: token response’a dönebilir (prod’da SMS/E-mail ile gönderilir)
    return Results.Ok(new
    {
        recoveryId = recovery.Id,
        credentialId = recovery.CredentialId,
        expiresAt = recovery.ExpiresAt,
        token = plainToken
    });
});

// POST /api/v1/identity/recovery/complete
group.MapPost("/recovery/complete", async (CompleteRecoveryBody body, IdentityService service, CancellationToken ct) =>
{
    await service.CompleteRecoveryAsync(body.Token, body.NewPassword, ct);
    return Results.NoContent();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public sealed record StartRegistrationBody(Guid CustomerId, string PhoneNumber, string Password);
public sealed record LoginBody(string PhoneNumber, string Password);
public sealed record StartRecoveryBody(string PhoneNumber, string Channel);
public sealed record CompleteRecoveryBody(string Token, string NewPassword);
