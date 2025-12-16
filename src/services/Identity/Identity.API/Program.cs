using System.Text;
using GaniPay.Identity.Application.Security;
using GaniPay.Identity.Application.Services;
using GaniPay.Identity.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Infrastructure (DbContext, Repo, JwtTokenGenerator, etc.)
// ------------------------------------------------------
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// ------------------------------------------------------
// App Services
// IMPORTANT: Singleton deðil. DB/Request scope için Scoped kullan.
// ------------------------------------------------------
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IIdentityService, IdentityService>(); // ÞU ANKI IdentityService in-memory => DB yazmaz!

// ------------------------------------------------------
// JWT Options (config'ten)
// ------------------------------------------------------
var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Jwt configuration missing. '{JwtOptions.SectionName}' section.");

// ------------------------------------------------------
// Authentication / Authorization
// ------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // local dev
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------------------
// Swagger + Bearer Authorize
// ------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GaniPay.Identity.API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token} formatýnda JWT girin.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ------------------------------------------------------
// DB migrate (opsiyonel ama önerilir)
// AddIdentityInfrastructure içinde DbContext varsa migration uygular.
// Burayý istemezsen kapatýrýz.
// ------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Örn: app.Services.ApplyIdentityMigrations(); gibi bir extension yazýlabilir.
    // Þimdilik migration yoksa bile patlatmaz.
    using var scope = app.Services.CreateScope();
    // Eðer IdentityDbContext sýnýf adýn farklýysa burayý uyarlayacaðýz.
    // var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    // db.Database.Migrate();
}

// ------------------------------------------------------
// Pipeline
// ------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------
// Health
// ------------------------------------------------------
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithTags("System");

// ------------------------------------------------------
// Routes
// ------------------------------------------------------
var group = app.MapGroup("/api/v1/identity")
               .WithTags("Identity");

// Bu endpointler eðer [Authorize] istiyorsan burada RequireAuthorization() ekle
// Örn: group.RequireAuthorization();  (login/register hariç ayrý yönetebiliriz)

group.MapPost("/registrations/start",
    (GaniPay.Identity.Application.Contracts.Requests.StartRegistrationRequest req, IIdentityService service) =>
    {
        var dto = service.StartRegistration(req);
        return Results.Created($"/api/v1/identity/credentials/{dto.Id}", dto);
    });

group.MapPost("/registrations/complete",
    (GaniPay.Identity.Application.Contracts.Requests.CompleteRegistrationRequest req, IIdentityService service) =>
    {
        var dto = service.CompleteRegistration(req);
        return Results.Ok(dto);
    });

group.MapPost("/login",
    (GaniPay.Identity.Application.Contracts.Requests.LoginRequest req, IIdentityService service) =>
    {
        var dto = service.Login(req);
        // NOT: Token üretimini IdentityService içine veya ayrý endpoint'e baðlayacaðýz.
        return Results.Ok(new { success = true, credential = dto });
    });

group.MapGet("/credentials/{id:guid}", (Guid id, IIdentityService service) =>
{
    var dto = service.GetCredentialById(id);
    return dto is not null ? Results.Ok(dto) : Results.NotFound();
});

group.MapGet("/credentials/by-phone/{phone}", (string phone, IIdentityService service) =>
{
    var dto = service.GetCredentialByPhone(phone);
    return dto is not null ? Results.Ok(dto) : Results.NotFound();
});

group.MapPost("/recovery/start",
    (GaniPay.Identity.Application.Contracts.Requests.StartRecoveryRequest req, IIdentityService service) =>
    {
        var (recovery, token) = service.StartRecovery(req);
        return Results.Ok(new { recovery, demo_token = token });
    });

group.MapPost("/recovery/complete",
    (GaniPay.Identity.Application.Contracts.Requests.CompleteRecoveryRequest req, IIdentityService service) =>
    {
        service.CompleteRecovery(req);
        return Results.NoContent();
    });

app.Run();
