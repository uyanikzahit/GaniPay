using System.Text.Json;
using System.Text.Json.Serialization;
using Zeebe.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON options (enumlarý string olarak gönderiyoruz)
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Zeebe config
var zeebeGateway = builder.Configuration["Zeebe:GatewayAddress"] ?? "127.0.0.1:26500";
var registerProcessId = builder.Configuration["Zeebe:RegisterProcessId"] ?? "register";
var loginProcessId = builder.Configuration["Zeebe:LoginProcessId"] ?? "login";

// Zeebe Client (Singleton)
builder.Services.AddSingleton<IZeebeClient>(_ =>
{
    return ZeebeClient.Builder()
        .UseGatewayAddress(zeebeGateway)
        .UsePlainText()
        .Build();
});

// Variables serializer (Zeebe’ye JSON string basacaðýz)
var zeebeJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
zeebeJsonOptions.Converters.Add(new JsonStringEnumConverter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", (IConfiguration cfg) =>
{
    return Results.Ok(new
    {
        status = "ok",
        service = "workflow",
        zeebeGateway = cfg["Zeebe:GatewayAddress"]
    });
});



//register
var group = app.MapGroup("/api/v1/onboarding")
    .WithTags("Onboarding");


//login
var authGroup = app.MapGroup("/api/v1/auth")
    .WithTags("Auth");

// POST /api/v1/onboarding/register
// Görev: Sadece Zeebe’de main flow’u baþlatmak.
// Customer/Identity/Accounting çaðrýlarýný Worker’lar yapacak.
group.MapPost("/register", async (
    RegisterRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    // CorrelationId: header varsa onu al, yoksa üret
    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    // Zeebe’ye gidecek variables (DataCreation/Validation tasklerinde lazým olan alanlarý koyuyoruz)
    // NOT: CustomerId burada yok, çünkü customer.create task’i customerId üretiyor.
    var variables = new
    {
        correlationId,

        // Register input
        firstName = req.FirstName,
        lastName = req.LastName,

        // DateOnly -> string (en güvenlisi)
        birthDate = req.BirthDate.ToString("yyyy-MM-dd"),

        nationality = req.Nationality,
        identityNumber = req.IdentityNumber,
        segment = req.Segment,

        // Identity için lazým
        phoneNumber = req.PhoneNumber,
        password = req.Password,

        // Customer email/address için lazým
        email = req.Email,
        address = new
        {
            addressType = req.Address.AddressType,
            city = req.Address.City,
            district = req.Address.District,
            postalCode = req.Address.PostalCode,
            addressLine1 = req.Address.AddressLine1
        },

        // Accounting için
        currency = req.Currency ?? "TRY"
    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        // Main flow baþlat
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(registerProcessId) // "register"
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        // 202 Accepted: instance baþladý, bundan sonrasý workflow+workerlar
        return Results.Accepted(
            value: new
            {
                success = true,
                correlationId,
                processId = registerProcessId,
                processInstanceKey = resp.ProcessInstanceKey
            });
    }
    catch (Exception ex)
    {
        // Zeebe’ye baðlanamama / deploy yok / processId yanlýþ vb.
        return Results.Problem(
            title: "Could not start register workflow",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});


authGroup.MapPost("/login", async (
    LoginRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    // Basit validasyon
    if (string.IsNullOrWhiteSpace(req.PhoneNumber) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "phoneNumber ve password zorunludur."
        });
    }

    // CorrelationId: header varsa onu al, yoksa üret
    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    // Login workerlarýnýn beklediði variable isimleriyle birebir
    var variables = new
    {
        correlationId,
        phoneNumber = req.PhoneNumber,
        password = req.Password,
        ipAddress = req.IpAddress,
        deviceId = req.DeviceId,
        channel = req.Channel,
        clientVersion = req.ClientVersion
    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(loginProcessId) // appsettings: "login"
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        return Results.Accepted(
        value: new
        {
            success = true,
            correlationId,
            processId = loginProcessId,
            processInstanceKey = resp.ProcessInstanceKey
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Could not start login workflow",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    // Zeebe client dispose
    if (app.Services.GetService<IZeebeClient>() is IDisposable d)
        d.Dispose();
});

app.Run();

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string? Nationality,
    string IdentityNumber,
    string Segment,
    string PhoneNumber,
    string Password,
    string Email,
    RegisterAddress Address,
    string? Currency
);

public sealed record RegisterAddress(
    string AddressType,
    string City,
    string District,
    string PostalCode,
    string AddressLine1
);


public sealed record LoginRequest(
    string PhoneNumber,
    string Password,
    string? IpAddress,
    string? DeviceId,
    string? Channel,
    string? ClientVersion
);
