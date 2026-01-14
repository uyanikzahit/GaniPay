using System.Text.Json;
using System.Text.Json.Serialization;
using Zeebe.Client;
using GaniPay.Workflow.API.ResultStore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ✅ CORS (builder.Build() öncesi)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8082", // Expo web
                "https://localhost:8082" // bazen https'e düşebilir
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // .AllowCredentials(); // sadece cookie/auth gerekiyorsa aç
    });
});


// JSON options (enumları string olarak gönderiyoruz)
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Zeebe config
var zeebeGateway = builder.Configuration["Zeebe:GatewayAddress"] ?? "127.0.0.1:26500";
var registerProcessId = builder.Configuration["Zeebe:RegisterProcessId"] ?? "register";
var loginProcessId = builder.Configuration["Zeebe:LoginProcessId"] ?? "login";
var topUpProcessId = builder.Configuration["Zeebe:TopUpProcessId"] ?? "topup";
var transferProcessId = builder.Configuration["Zeebe:TransferProcessId"] ?? "transfer";

// Zeebe Client (Singleton)
builder.Services.AddSingleton<IZeebeClient>(_ =>
{
     return  ZeebeClient.Builder()
         .UseGatewayAddress(zeebeGateway)
        .UsePlainText()
        .Build();
});

// Variables serializer (Zeebe’ye JSON string basacağız)
var zeebeJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
zeebeJsonOptions.Converters.Add(new JsonStringEnumConverter());

var app = builder.Build();


// ✅ CORS (Map*’lerden önce)
app.UseCors("Frontend");

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

// -------------------- GROUPS --------------------
var onboardingGroup = app.MapGroup("/api/v1/onboarding").WithTags("Onboarding");
var authGroup = app.MapGroup("/api/v1/auth").WithTags("Auth");
var paymentsGroup = app.MapGroup("/api/v1/payments").WithTags("Payments");
var transfersGroup = app.MapGroup("/api/v1/transfers").WithTags("Transfers");

// -------------------- REGISTER --------------------
onboardingGroup.MapPost("/register", async (
    RegisterRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    // Basit validasyon (istersen arttır)
    if (string.IsNullOrWhiteSpace(req.PhoneNumber) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new { success = false, message = "phoneNumber ve password zorunludur." });
    }

    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    // Zeebe variables (register worker’larının beklediği alanlar)
    var variables = new
    {
        correlationId,
        firstName = req.FirstName,
        lastName = req.LastName,
        birthDate = req.BirthDate.ToString("yyyy-MM-dd"),
        nationality = req.Nationality,
        identityNumber = req.IdentityNumber,
        segment = req.Segment,
        phoneNumber = req.PhoneNumber,
        password = req.Password,
        email = req.Email,
        address = new
        {
            addressType = req.Address.AddressType,
            city = req.Address.City,
            district = req.Address.District,
            postalCode = req.Address.PostalCode,
            addressLine1 = req.Address.AddressLine1
        },
        currency = req.Currency ?? "TRY"
    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(registerProcessId)
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

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
        return Results.Problem(
            title: "Could not start register workflow",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// -------------------- LOGIN --------------------
authGroup.MapPost("/login", async (
    LoginRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.PhoneNumber) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new
        {
            success = false,
            status = "Failed",
            message = "phoneNumber ve password zorunludur."
        });
    }

    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    // ✅ Aynı correlationId ile daha önce store’a yazılmış eski bir sonuç varsa temizle
    LoginResultStore.Remove(correlationId);

    var variables = new
    {
        correlationId,
        phoneNumber = req.PhoneNumber,
        password = req.Password

    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        // 1) Workflow başlat
        await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(loginProcessId)
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        // 2) Kısa süre bekle (max 2 sn) -> store’a sonuç düştüyse dön
        for (int i = 0; i < 20; i++)
        {
            if (LoginResultStore.TryGet(correlationId, out var result))
            {
                // ✅ Okunduktan sonra sil (aynı correlationId tekrar kullanılırsa karışmasın)
                LoginResultStore.Remove(correlationId);

                return Results.Ok(new
                {
                    success = result.Success,
                    status = result.Status,   // "Succeeded" | "Failed"
                    message = result.Message,
                    token = result.Token,
                    correlationId
                });
            }

            await Task.Delay(100, ct);
        }

        // 3) Hâlâ yoksa -> workflow çalışıyor (başarılı DEME!)
        // ✅ 202 Accepted dön: UI bunu "bekle/poll et" diye anlayacak
        return Results.Accepted(value: new
        {
            success = false,
            status = "Running",
            correlationId,
            message = "Login is being processed."
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


authGroup.MapPost("/login/result", (LoginResultCallback body) =>
{
    if (string.IsNullOrWhiteSpace(body.CorrelationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    LoginResultStore.Set(
        body.CorrelationId,
        new LoginResult(body.Success, body.Status, body.Message, body.Token)
    );

    return Results.Ok(new { success = true });
});







// -------------------- TOPUP --------------------
paymentsGroup.MapPost("/topup", async (
    TopUpRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.CustomerId) ||
        string.IsNullOrWhiteSpace(req.AccountId) ||
        string.IsNullOrWhiteSpace(req.Currency) ||
        string.IsNullOrWhiteSpace(req.IdempotencyKey) ||
        string.IsNullOrWhiteSpace(req.ReferenceId) ||
        req.Amount <= 0)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "customerId, accountId, amount(>0), currency, idempotencyKey, referenceId zorunludur."
        });
    }

    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    var variables = new
    {
        correlationId,
        customerId = req.CustomerId,
        accountId = req.AccountId,
        amount = req.Amount,
        currency = req.Currency,
        idempotencyKey = req.IdempotencyKey,
        referenceId = req.ReferenceId
    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(topUpProcessId)
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        return Results.Accepted(
            value: new
            {
                success = true,
                correlationId,
                processId = topUpProcessId,
                processInstanceKey = resp.ProcessInstanceKey
            });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Could not start topup workflow",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// -------------------- TRANSFER --------------------
transfersGroup.MapPost("/transfer", async (
    TransferRequest req,
    HttpContext http,
    IZeebeClient zeebe,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.CustomerId) ||
        string.IsNullOrWhiteSpace(req.ReceiverCustomerId) ||
        req.Amount <= 0)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "customerId, receiverCustomerId, amount(>0) zorunludur."
        });
    }

    var correlationId =
        http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

    // Modeler’da verdiğin JSON isimleriyle birebir
    var variables = new
    {
        correlationId,
        customerId = req.CustomerId,
        receiverCustomerId = req.ReceiverCustomerId,
        amount = req.Amount
    };

    var variablesJson = JsonSerializer.Serialize(variables, zeebeJsonOptions);

    try
    {
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(transferProcessId)
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        return Results.Accepted(
            value: new
            {
                success = true,
                correlationId,
                processId = transferProcessId,
                processInstanceKey = resp.ProcessInstanceKey
            });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Could not start transfer workflow",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    if (app.Services.GetService<IZeebeClient>() is IDisposable d)
        d.Dispose();
});

app.Run();

// -------------------- DTOs --------------------
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

public sealed record LoginResultCallback(
    string CorrelationId,
    bool Success,
    string Status,
    string Message,
    string? Token
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
    string Password
);

public sealed record TopUpRequest(
    string CustomerId,
    string AccountId,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    string ReferenceId
);

public sealed record TransferRequest(
    string CustomerId,
    string ReceiverCustomerId,
    decimal Amount
);