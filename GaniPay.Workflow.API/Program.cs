using System.Text.Json;
using System.Text.Json.Serialization;
using Zeebe.Client;
using GaniPay.Workflow.API.ResultStore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8082", 
                "https://localhost:8082" 
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
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
    return ZeebeClient.Builder()
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
        for (int i = 0; i < 80; i++) // 8 saniye
        {
            if (LoginResultStore.TryGet(correlationId, out var result))
            {
                return Results.Ok(new
                {
                    success = result.Success,
                    status = result.Status,   // "Succeeded" | "Failed"
                    message = result.Message,
                    token = result.Token,

                    // ✅ yeni alanlar
                    customerId = result.CustomerId,
                    customer = result.Customer,
                    wallets = result.Wallets,

                    correlationId
                });
            }

            await Task.Delay(100, ct);
        }

        // 3) Hâlâ yoksa -> workflow çalışıyor (başarılı DEME!)
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


authGroup.MapPost("/login/callback", (LoginResultCallback body) =>
{
    if (string.IsNullOrWhiteSpace(body.CorrelationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    LoginResultStore.Set(
        body.CorrelationId,
        new LoginResult(
            Success: body.Success,
            Status: body.Status,
            Message: body.Message,
            Token: body.Token,
            CustomerId: body.CustomerId,
            Customer: body.Customer,
            Wallets: body.Wallets
        )
    );

    return Results.Ok(new { success = true });
});


authGroup.MapGet("/login/result/{correlationId}", (string correlationId) =>
{
    if (string.IsNullOrWhiteSpace(correlationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    if (LoginResultStore.TryGet(correlationId, out var result))
    {
        LoginResultStore.Remove(correlationId);

        return Results.Ok(new
        {
            success = result.Success,
            status = result.Status,
            message = result.Message,
            token = result.Token,
            customerId = result.CustomerId,
            customer = result.Customer,
            wallets = result.Wallets,
            correlationId
        });
    }

    // ❗️404 verme, 200 dön ki UI hata sanmasın
    return Results.Ok(new
    {
        success = false,
        status = "Running",
        message = "Login is being processed.",
        correlationId
    });
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

    // ✅ Aynı correlationId ile daha önce store’a yazılmış eski bir sonuç varsa temizle
    TopUpResultStore.Remove(correlationId);

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
        // 1) Workflow başlat
        var resp = await zeebe.NewCreateProcessInstanceCommand()
            .BpmnProcessId(topUpProcessId)
            .LatestVersion()
            .Variables(variablesJson)
            .Send();

        // 2) Kısa süre bekle -> callback geldiyse aynı response’ta sonucu dön
        for (int i = 0; i < 80; i++) // 8 saniye (80 * 100ms)
        {
            if (TopUpResultStore.TryGet(correlationId, out var result))
            {
                TopUpResultStore.Remove(correlationId);

                var message = result.Success
                    ? "Top up completed successfully."
                    : "Top up failed.";

                if (!string.IsNullOrWhiteSpace(result.Message))
                    message = result.Message;

                return Results.Ok(new
                {
                    success = result.Success,
                    status = result.Success ? "Succeeded" : "Failed",
                    message,
                    data = result.Data,
                    correlationId
                });
            }

            await Task.Delay(100, ct);
        }

        // 3) Hâlâ yoksa -> workflow çalışıyor
        return Results.Accepted(value: new
        {
            success = false,
            status = "Running",
            correlationId,
            processId = topUpProcessId,
            processInstanceKey = resp.ProcessInstanceKey,
            message = "Top up is being processed."
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


paymentsGroup.MapPost("/topup/result", (TopUpResultCallback body) =>
{
    if (string.IsNullOrWhiteSpace(body.CorrelationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    var normalizedStatus =
        string.IsNullOrWhiteSpace(body.Status)
            ? (body.Success ? "Succeeded" : "Failed")
            : body.Status;

    var normalizedMessage =
        string.IsNullOrWhiteSpace(body.Message)
            ? (body.Success ? "Top up completed successfully." : "Top up failed.")
            : body.Message;

    TopUpResultStore.Set(
        body.CorrelationId,
        new TopUpResult(
            Success: body.Success,
            Status: normalizedStatus,
            Message: normalizedMessage,
            Data: body.Data
        )
    );

    return Results.Ok(new { success = true });
});

paymentsGroup.MapGet("/topup/result/{correlationId}", (string correlationId) =>
{
    if (string.IsNullOrWhiteSpace(correlationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    if (TopUpResultStore.TryGet(correlationId, out var result))
    {
        TopUpResultStore.Remove(correlationId);

        return Results.Ok(new
        {
            success = result.Success,
            status = result.Status,
            message = result.Message,
            data = result.Data,
            correlationId
        });
    }

    return Results.NotFound(new
    {
        success = false,
        status = "Running",
        message = "Top up is being processed.",
        correlationId
    });
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

    TransferResultStore.Remove(correlationId);

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


transfersGroup.MapPost("/transfer/result", (TransferResultCallback body) =>
{
    if (string.IsNullOrWhiteSpace(body.CorrelationId))
        return Results.BadRequest(new { success = false, message = "correlationId zorunlu" });

    TransferResultStore.Set(
        body.CorrelationId,
        new TransferResult(
            Success: body.Success,
            Status: body.Status,
            Message: body.Message,
            Data: body.Data
        )
    );

    return Results.Ok(new { success = true });
});

transfersGroup.MapGet("/transfer/result/{correlationId}", (string correlationId) =>
{
    if (TransferResultStore.TryGet(correlationId, out var result))
    {
        TransferResultStore.Remove(correlationId);

        return Results.Ok(new
        {
            success = result.Success,
            status = result.Status,
            message = result.Message,
            data = result.Data,
            correlationId
        });
    }

    return Results.NotFound(new
    {
        success = false,
        status = "Running",
        message = "Transfer is being processed.",
        correlationId
    });
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
    string? Token,

    string? CustomerId,
    object? Customer,
    object? Wallets
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

public sealed record TopUpResultCallback(
    string CorrelationId,
    bool Success,
    string Status,
    string Message,
    object? Data
);

public sealed record TransferResultCallback(
    string CorrelationId,
    bool Success,
    string Status,
    string Message,
    object? Data
);
