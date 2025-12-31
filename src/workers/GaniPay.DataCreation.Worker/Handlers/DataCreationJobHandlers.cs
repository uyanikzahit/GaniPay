using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.DataCreation.Worker.Handlers;

public sealed class DataCreationJobHandlers
{
    private readonly ILogger<DataCreationJobHandlers> _logger;
    private readonly IHttpClientFactory _http;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public DataCreationJobHandlers(
        ILogger<DataCreationJobHandlers> logger,
        IHttpClientFactory http)
    {
        _logger = logger;
        _http = http;
    }

    // -------------------------
    // Helpers
    // -------------------------
    private static string Vars(IJob job)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(job.Variables)) return "{}";
            using var doc = JsonDocument.Parse(job.Variables);
            return doc.RootElement.ToString();
        }
        catch
        {
            return job.Variables ?? "{}";
        }
    }

    private static string GetString(IJob job, string key)
    {
        if (string.IsNullOrWhiteSpace(job.Variables)) return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(job.Variables);
            if (!doc.RootElement.TryGetProperty(key, out var p)) return string.Empty;

            return p.ValueKind switch
            {
                JsonValueKind.String => p.GetString() ?? string.Empty,
                JsonValueKind.Number => p.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => p.GetRawText()
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string? GetOrNull(IJob job, string key)
    {
        var s = GetString(job, key);
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static Guid ExtractGuid(string json, string prop)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty(prop, out var p))
            throw new InvalidOperationException($"Response does not contain '{prop}'. Raw: {json}");

        if (p.ValueKind == JsonValueKind.String)
            return Guid.Parse(p.GetString()!);

        return p.GetGuid();
    }

    private static void Complete(IJobClient client, IJob job, Dictionary<string, object> variables)
    {
        var json = JsonSerializer.Serialize(variables, JsonOpts);

        client.NewCompleteJobCommand(job.Key)
            .Variables(json) // string JSON
            .Send()
            .GetAwaiter()
            .GetResult();
    }

    // -------------------------
    // 1) accounting.wallet.ledgerid.get (MVP mock)
    // -------------------------
    public void HandleWalletAccountLedgerIdGet(IJobClient client, IJob job)
    {
        _logger.LogInformation("[accounting.wallet.ledgerid.get] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var vars = new Dictionary<string, object>
        {
            ["ledgerId"] = Guid.NewGuid().ToString()
        };

        Complete(client, job, vars);

        _logger.LogInformation("[accounting.wallet.ledgerid.get] COMPLETED key={Key} ledgerId={LedgerId}",
            job.Key, vars["ledgerId"]);
    }

    // -------------------------
    // 2) customer.create (GERÇEK)
    // POST https://localhost:7101/api/v1/customers/individual
    // response: { id: "..." }
    // -------------------------
    public void HandleCustomerCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customer = _http.CreateClient("customer");

        var body = new
        {
            firstName = GetString(job, "firstName"),
            lastName = GetString(job, "lastName"),
            birthDate = GetString(job, "birthDate"),
            nationality = GetOrNull(job, "nationality"),
            identityNumber = GetString(job, "identityNumber"),
            segment = GetString(job, "segment")
        };

        var resp = customer.PostAsJsonAsync("/api/v1/customers/individual", body)
                           .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer returned {(int)resp.StatusCode}: {raw}");

        var customerId = ExtractGuid(raw, "id").ToString();

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerId"] = customerId
        });

        _logger.LogInformation("[customer.create] COMPLETED key={Key} customerId={CustomerId}", job.Key, customerId);
    }

    // -------------------------
    // 3) customer.individual.create (NO-OP)
    // customer.create zaten individual create yaptığı için burada sadece complete ediyoruz.
    // -------------------------
    public void HandleCustomerIndividualCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.individual.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerIndividualCreated"] = true
        });

        _logger.LogInformation("[customer.individual.create] COMPLETED key={Key}", job.Key);
    }

    // -------------------------
    // 4) customer.email.create (GERÇEK)
    // POST /api/v1/customers/{customerId}/emails
    // -------------------------
    public void HandleCustomerEmailCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.email.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customerId = GetString(job, "customerId");
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");

        var customer = _http.CreateClient("customer");

        var body = new
        {
            emailAddress = GetString(job, "emailAddress"),
            type = int.TryParse(GetString(job, "emailType"), out var t) ? t : 1
        };

        var resp = customer.PostAsJsonAsync($"/api/v1/customers/{customerId}/emails", body)
                           .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer email returned {(int)resp.StatusCode}: {raw}");

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerEmailCreated"] = true
        });

        _logger.LogInformation("[customer.email.create] COMPLETED key={Key} customerId={CustomerId}", job.Key, customerId);
    }

    // -------------------------
    // 5) customer.phone.create (GERÇEK)
    // POST /api/v1/customers/{customerId}/phones
    // -------------------------
    public void HandleCustomerPhoneCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.phone.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customerId = GetString(job, "customerId");
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");

        var customer = _http.CreateClient("customer");

        var body = new
        {
            countryCode = GetString(job, "countryCode"),
            phoneNumber = GetString(job, "phoneNumber"),
            type = int.TryParse(GetString(job, "phoneType"), out var t) ? t : 1
        };

        var resp = customer.PostAsJsonAsync($"/api/v1/customers/{customerId}/phones", body)
                           .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer phone returned {(int)resp.StatusCode}: {raw}");

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerPhoneCreated"] = true
        });

        _logger.LogInformation("[customer.phone.create] COMPLETED key={Key} customerId={CustomerId}", job.Key, customerId);
    }

    // -------------------------
    // 6) customer.address.create (GERÇEK)
    // POST /api/v1/customers/{customerId}/addresses
    // -------------------------
    public void HandleCustomerAddressCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.address.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customerId = GetString(job, "customerId");
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");

        var customer = _http.CreateClient("customer");

        var body = new
        {
            addressType = int.TryParse(GetString(job, "addressType"), out var at) ? at : 1,
            city = GetString(job, "city"),
            district = GetString(job, "district"),
            postalCode = GetString(job, "postalCode"),
            addressLine1 = GetString(job, "addressLine1")
        };

        var resp = customer.PostAsJsonAsync($"/api/v1/customers/{customerId}/addresses", body)
                           .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer address returned {(int)resp.StatusCode}: {raw}");

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerAddressCreated"] = true
        });

        _logger.LogInformation("[customer.address.create] COMPLETED key={Key} customerId={CustomerId}", job.Key, customerId);
    }

    // -------------------------
    // 7) mock.device.customer.link (mock)
    // -------------------------
    public void HandleLinkDeviceAndCustomer(IJobClient client, IJob job)
    {
        _logger.LogInformation("[mock.device.customer.link] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        Complete(client, job, new Dictionary<string, object>
        {
            ["deviceCustomerLinkId"] = Guid.NewGuid().ToString()
        });

        _logger.LogInformation("[mock.device.customer.link] COMPLETED key={Key}", job.Key);
    }

    // -------------------------
    // 8) accounting.account.create (GERÇEK)
    // POST http://localhost:5103/api/accounting/accounts
    // response: { id: "...", accountNumber: "..." }
    // -------------------------
    public void HandleAccountCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[accounting.account.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customerId = GetString(job, "customerId");
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");

        var accounting = _http.CreateClient("accounting");

        var body = new
        {
            customerId = customerId,
            currency = GetOrNull(job, "currency") ?? "TRY",
            iban = GetOrNull(job, "iban") ?? "string"
        };

        var resp = accounting.PostAsJsonAsync("/api/accounting/accounts", body)
                             .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Accounting returned {(int)resp.StatusCode}: {raw}");

        var accountId = ExtractGuid(raw, "id").ToString();

        string accountNumber = "";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("accountNumber", out var p))
                accountNumber = p.GetString() ?? "";
        }
        catch { }

        Complete(client, job, new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["accountNumber"] = accountNumber
        });

        _logger.LogInformation("[accounting.account.create] COMPLETED key={Key} accountId={AccountId}", job.Key, accountId);
    }

    // -------------------------
    // 9) mock.customer.occupation.create (mock)
    // -------------------------
    public void HandleCustomerOccupationCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[mock.customer.occupation.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        Complete(client, job, new Dictionary<string, object>
        {
            ["customerOccupationId"] = Guid.NewGuid().ToString()
        });

        _logger.LogInformation("[mock.customer.occupation.create] COMPLETED key={Key}", job.Key);
    }

    // -------------------------
    // 10) identity.credential.create (GERÇEK)
    // POST http://localhost:5102/api/v1/identity/registrations/start
    // response: { id: "..." }
    // -------------------------
    public void HandleIdentityCredentialCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[identity.credential.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        var customerId = GetString(job, "customerId");
        var phoneNumber = GetString(job, "phoneNumber");
        var password = GetString(job, "password");

        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException("phoneNumber variable is missing");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("password variable is missing");

        var identity = _http.CreateClient("identity");

        var body = new
        {
            customerId = Guid.Parse(customerId),
            phoneNumber,
            password
        };

        var resp = identity.PostAsJsonAsync("/api/v1/identity/registrations/start", body)
                           .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Identity returned {(int)resp.StatusCode}: {raw}");

        var credentialId = ExtractGuid(raw, "id").ToString();

        Complete(client, job, new Dictionary<string, object>
        {
            ["credentialId"] = credentialId
        });

        _logger.LogInformation("[identity.credential.create] COMPLETED key={Key} credentialId={CredentialId}", job.Key, credentialId);
    }
}