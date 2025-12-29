using System.Net.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Downstream base urls
var customerBaseUrl = builder.Configuration["Downstream:Customer:BaseUrl"] ?? "https://localhost:7101";
var identityBaseUrl = builder.Configuration["Downstream:Identity:BaseUrl"] ?? "http://localhost:5102";
var accountingBaseUrl = builder.Configuration["Downstream:Accounting:BaseUrl"] ?? "http://localhost:5103";

// Named HttpClients
builder.Services.AddHttpClient("customer", c =>
{
    c.BaseAddress = new Uri(customerBaseUrl);
});

builder.Services.AddHttpClient("identity", c =>
{
    c.BaseAddress = new Uri(identityBaseUrl);
});

builder.Services.AddHttpClient("accounting", c =>
{
    c.BaseAddress = new Uri(accountingBaseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "workflow" }));

var group = app.MapGroup("/api/v1/onboarding")
    .WithTags("Onboarding");

// POST /api/v1/onboarding/register
group.MapPost("/register", async (
    RegisterRequest req,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    // 1) Customer oluþtur
    var customerClient = httpClientFactory.CreateClient("customer");

    // Customer.API’nin beklediði payload alan isimleri sende farklýysa burada map et.
    var customerCreateBody = new
    {
        firstName = req.FirstName,
        lastName = req.LastName,
        birthDate = req.BirthDate,
        nationality = req.Nationality,
        identityNumber = req.IdentityNumber,
        segment = req.Segment,
        // zorunlu email + address
        email = req.Email,
        address = new
        {
            addressType = req.Address.AddressType,
            city = req.Address.City,
            district = req.Address.District,
            postalCode = req.Address.PostalCode,
            addressLine1 = req.Address.AddressLine1
        }
    };

    HttpResponseMessage customerResp;
    try
    {
        customerResp = await customerClient.PostAsJsonAsync("/api/v1/customers/individual", customerCreateBody, ct);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "Customer service unreachable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!customerResp.IsSuccessStatusCode)
    {
        var err = await customerResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Customer create failed",
            detail: err,
            statusCode: (int)customerResp.StatusCode);
    }

    var createdCustomer = await customerResp.Content.ReadFromJsonAsync<CustomerCreatedResponse>(cancellationToken: ct);
    if (createdCustomer is null || createdCustomer.Id == Guid.Empty)
    {
        return Results.Problem(
            title: "Customer create failed",
            detail: "Customer response could not be parsed.");
    }

    // 2) Identity credential oluþtur (customerId ile)
    var identityClient = httpClientFactory.CreateClient("identity");

    var identityStartBody = new
    {
        customerId = createdCustomer.Id,
        phoneNumber = req.PhoneNumber,
        password = req.Password
    };

    HttpResponseMessage identityResp;
    try
    {
        identityResp = await identityClient.PostAsJsonAsync("/api/v1/identity/registrations/start", identityStartBody, ct);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "Identity service unreachable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!identityResp.IsSuccessStatusCode)
    {
        var err = await identityResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Identity registration failed",
            detail: err,
            statusCode: (int)identityResp.StatusCode);
    }

    var createdCredential = await identityResp.Content.ReadFromJsonAsync<CredentialCreatedResponse>(cancellationToken: ct);
    if (createdCredential is null || createdCredential.Id == Guid.Empty)
    {
        return Results.Problem(
            title: "Identity registration failed",
            detail: "Identity response could not be parsed.");
    }

    // 3) Accounting wallet account oluþtur (customerId ile)
    var accountingClient = httpClientFactory.CreateClient("accounting");

    // Accounting endpointin þu an accountNumber istiyorsa, sen düzenleyene kadar geçici olarak dummy gönderebilirsin.
    // Ama sen accountNumber'ý sistem üretecek þekilde düzeltiyorum dediðin için burada accountNumber yok.
    var accountingCreateBody = new
    {
        customerId = createdCustomer.Id,
        currency = req.Currency ?? "TRY"
    };

    HttpResponseMessage accResp;
    try
    {
        accResp = await accountingClient.PostAsJsonAsync("/api/accounting/accounts", accountingCreateBody, ct);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "Accounting service unreachable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!accResp.IsSuccessStatusCode)
    {
        var err = await accResp.Content.ReadAsStringAsync(ct);
        return Results.Problem(
            title: "Accounting account create failed",
            detail: err,
            statusCode: (int)accResp.StatusCode);
    }

    // Accounting response modelin birebir bilinmiyor; parse etmeyi esnek tutalým:
    // Eðer accounting dönen response'u biliyorsan bunu kendi DTO’n ile deðiþtir.
    var accJson = await accResp.Content.ReadAsStringAsync(ct);

    // 4) MVP response: customerId + credentialId + accounting raw response
    return Results.Created($"/api/v1/customers/{createdCustomer.Id}", new
    {
        success = true,
        customerId = createdCustomer.Id,
        credentialId = createdCredential.Id,
        currency = accountingCreateBody.currency,
        accounting = accJson
    });
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

// Customer.API response (minimum)
public sealed record CustomerCreatedResponse(Guid Id);

// Identity response (minimum)
public sealed record CredentialCreatedResponse(Guid Id, Guid CustomerId);
