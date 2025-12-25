using System.Net.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ---------------------------
// Downstream service base urls
// ---------------------------
var customerBaseUrl = builder.Configuration["Services:CustomerBaseUrl"] ?? "https://localhost:7101";
var identityBaseUrl = builder.Configuration["Services:IdentityBaseUrl"] ?? "http://localhost:5102";

// ---------------------------
// HttpClients
// ---------------------------

// Customer API (HTTPS -> DEV ortamýnda sertifika bypass)
builder.Services.AddHttpClient("customer", c =>
{
    c.BaseAddress = new Uri(customerBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

// Identity API (HTTP)
builder.Services.AddHttpClient("identity", c =>
{
    c.BaseAddress = new Uri(identityBaseUrl);
});

var app = builder.Build();

// Swagger: launchSettings.json zaten swagger'ý otomatik açýyor.
// Burada sadece pipeline'ý aktif ediyoruz.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "workflow" }));

var group = app.MapGroup("/api/v1/onboarding").WithTags("Onboarding");

// ---------------------------
// POST /api/v1/onboarding/register
// ---------------------------
group.MapPost("/register", async (
    RegisterRequest req,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    // 1) Customer oluþtur
    var customerClient = httpClientFactory.CreateClient("customer");

    var customerResp = await customerClient.PostAsJsonAsync("/api/v1/customers/individual", new
    {
        firstName = req.FirstName,
        lastName = req.LastName,
        birthDate = req.BirthDate,
        nationality = req.Nationality,
        identityNumber = req.IdentityNumber,
        segment = req.Segment,

        // Zorunlu dedin:
        email = req.Email,

        // Address zorunlu
        address = new
        {
            addressType = req.Address.AddressType,
            city = req.Address.City,
            district = req.Address.District,
            postalCode = req.Address.PostalCode,
            addressLine1 = req.Address.AddressLine1
        },

        // Ýstersen Customer create DTO’unda phone isteniyorsa burada da gönderebiliriz:
        // phoneNumber = req.PhoneNumber
    }, ct);

    if (!customerResp.IsSuccessStatusCode)
    {
        var errorBody = await customerResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Customer create failed",
            detail: errorBody,
            statusCode: (int)customerResp.StatusCode);
    }

    var createdCustomer = await customerResp.Content.ReadFromJsonAsync<CustomerCreatedResponse>(cancellationToken: ct);
    if (createdCustomer is null || createdCustomer.Id == Guid.Empty)
        return Results.Problem(title: "Customer create failed", detail: "Customer response is empty or invalid.");

    // 2) Identity registration start (CustomerId dýþarýdan veriliyor)
    var identityClient = httpClientFactory.CreateClient("identity");

    var regStartResp = await identityClient.PostAsJsonAsync("/api/v1/identity/registrations/start", new
    {
        customerId = createdCustomer.Id,
        phoneNumber = req.PhoneNumber,
        password = req.Password
    }, ct);

    if (!regStartResp.IsSuccessStatusCode)
    {
        var errorBody = await regStartResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Identity registration failed",
            detail: errorBody,
            statusCode: (int)regStartResp.StatusCode);
    }

    // 3) Register sonrasý login -> access token
    var loginResp = await identityClient.PostAsJsonAsync("/api/v1/identity/login", new
    {
        phoneNumber = req.PhoneNumber,
        password = req.Password
    }, ct);

    if (!loginResp.IsSuccessStatusCode)
    {
        var errorBody = await loginResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Login failed after registration",
            detail: errorBody,
            statusCode: (int)loginResp.StatusCode);
    }

    var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);

    return Results.Ok(new
    {
        success = true,
        customerId = createdCustomer.Id,
        accessToken = login?.AccessToken,
        expiresIn = login?.ExpiresIn ?? 3600
    });
})
.WithName("OnboardingRegister");

app.Run();

// ---------------------------
// Models
// ---------------------------
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
    RegisterAddress Address
);

public sealed record RegisterAddress(
    string AddressType,
    string City,
    string District,
    string PostalCode,
    string AddressLine1
);

// Customer create response: senin CustomerService CreateIndividualAsync dönüþünde { id } var
public sealed record CustomerCreatedResponse(Guid Id);

// Identity login response: senin swagger ekranýnda AccessToken dönüyor
public sealed record LoginResponse(string? AccessToken, int ExpiresIn);
