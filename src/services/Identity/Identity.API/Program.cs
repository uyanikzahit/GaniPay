using GaniPay.Identity.Application.Contracts.Requests;
using GaniPay.Identity.Application.Security;
using GaniPay.Identity.Application.Services;
using GaniPay.Identity.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// DI (MVP: In-memory IdentityService)
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IIdentityService, IdentityService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithTags("System");

var group = app.MapGroup("/api/v1/identity")
               .WithTags("Identity");

// POST /api/v1/identity/registrations/start
group.MapPost("/registrations/start", (StartRegistrationRequest req, IIdentityService service) =>
{
    try
    {
        var dto = service.StartRegistration(req);
        return Results.Created($"/api/v1/identity/credentials/{dto.Id}", dto);
    }
    catch (Exception)
    {
        return Results.BadRequest(new { message = "Registration start failed." });
    }
});

// POST /api/v1/identity/registrations/complete
group.MapPost("/registrations/complete", (CompleteRegistrationRequest req, IIdentityService service) =>
{
    try
    {
        var dto = service.CompleteRegistration(req);
        return Results.Ok(dto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { message = "Credential not found." });
    }
    catch (Exception)
    {
        return Results.BadRequest(new { message = "Registration completion failed." });
    }
});

// POST /api/v1/identity/login
group.MapPost("/login", (LoginRequest req, IIdentityService service) =>
{
    try
    {
        var dto = service.Login(req);

        // MVP: JWT yok. Þimdilik credential dönüyoruz.
        return Results.Ok(new { success = true, credential = dto });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception)
    {
        return Results.BadRequest(new { message = "Login failed." });
    }
});

// GET /api/v1/identity/credentials/{id}
group.MapGet("/credentials/{id:guid}", (Guid id, IIdentityService service) =>
{
    var dto = service.GetCredentialById(id);
    return dto is not null
        ? Results.Ok(dto)
        : Results.NotFound(new { message = "Credential not found", id });
});

// GET /api/v1/identity/credentials/by-phone/{phone}
group.MapGet("/credentials/by-phone/{phone}", (string phone, IIdentityService service) =>
{
    var dto = service.GetCredentialByPhone(phone);
    return dto is not null
        ? Results.Ok(dto)
        : Results.NotFound(new { message = "Credential not found", phone });
});

// POST /api/v1/identity/recovery/start
group.MapPost("/recovery/start", (StartRecoveryRequest req, IIdentityService service) =>
{
    try
    {
        var (recovery, token) = service.StartRecovery(req);

        // Demo/MVP: token'ý response döndürüyoruz.
        // Prod: token SMS/Email ile gider, response'ta gösterilmez.
        return Results.Ok(new { recovery, demo_token = token });
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { message = "Credential not found." });
    }
    catch (Exception)
    {
        return Results.BadRequest(new { message = "Recovery start failed." });
    }
});

// POST /api/v1/identity/recovery/complete
group.MapPost("/recovery/complete", (CompleteRecoveryRequest req, IIdentityService service) =>
{
    try
    {
        service.CompleteRecovery(req);
        return Results.NoContent();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception)
    {
        return Results.BadRequest(new { message = "Recovery completion failed." });
    }
});

app.Run();
