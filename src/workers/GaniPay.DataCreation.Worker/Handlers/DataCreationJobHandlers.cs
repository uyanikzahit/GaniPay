using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
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

    // 4) customer.email.create (GERÇEK)
    // POST /api/v1/customers/{customerId}/emails
    public void HandleCustomerEmailCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.email.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        // 1) customerId kesin lazım
        var customerId = GetString(job, "customerId");
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId variable is missing");

        // 2) job variables dictionary
        var vars = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables)
                   ?? new Dictionary<string, object>();

        // 3) emailAddress/email oku
        string? emailAddress = null;

        if (vars.TryGetValue("emailAddress", out var eaObj) && eaObj != null)
            emailAddress = eaObj.ToString();

        if (string.IsNullOrWhiteSpace(emailAddress) &&
            vars.TryGetValue("email", out var eObj) && eObj != null)
            emailAddress = eObj.ToString();

        if (string.IsNullOrWhiteSpace(emailAddress))
            throw new InvalidOperationException("emailAddress/email variable is missing");

        // 4) emailType/type oku (default 1)
        int emailType = 1;

        object? emailTypeObj = null;
        if (vars.TryGetValue("emailType", out var etObj) && etObj != null) emailTypeObj = etObj;
        else if (vars.TryGetValue("type", out var tObj) && tObj != null) emailTypeObj = tObj;

        if (emailTypeObj != null)
        {
            if (emailTypeObj is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var i)) emailType = i;
                else if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var s)) emailType = s;
                else if (je.ValueKind == JsonValueKind.True) emailType = 1;
                else if (je.ValueKind == JsonValueKind.False) emailType = 0;
            }
            else if (int.TryParse(emailTypeObj.ToString(), out var parsed))
            {
                emailType = parsed;
            }
        }

        // 5) Customer API call
        var customer = _http.CreateClient("customer");

        var body = new
        {
            emailAddress = emailAddress,
            type = emailType
        };

        var resp = customer.PostAsJsonAsync($"/api/v1/customers/{customerId}/emails", body)
            .GetAwaiter().GetResult();

        var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer email returned {(int)resp.StatusCode}: {raw}");

        // 6) Complete job
        Complete(client, job, new Dictionary<string, object>
        {
            ["customerEmailCreated"] = true
        });

        _logger.LogInformation("[customer.email.create] COMPLETED key={Key} customerId={CustomerId} email={Email}",
            job.Key, customerId, emailAddress);
    }

    // 5) customer.phone.create
    // POST /api/v1/customers/{customerId}/phones
    public async Task HandleCustomerPhoneCreate(IJobClient client, IJob job)
    {
        // 1) BPMN Input Mapping: customerId, countryCode, phoneNumber, phoneType
        var customerId = GetString(job, "customerId");
        var countryCode = GetString(job, "countryCode");
        var phoneNumber = GetString(job, "phoneNumber");
        var phoneTypeStr = GetString(job, "phoneType");

        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId missing");
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new InvalidOperationException("countryCode missing");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException("phoneNumber missing");

        // Swagger’da çalışan formata yaklaştır
        phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

        var type = int.TryParse(phoneTypeStr, out var t) ? t : 1;

        var http = _http.CreateClient("customer");

        // Customer API: POST /api/v1/customers/{customerId}/phones
        var url = $"/api/v1/customers/{customerId}/phones";

        var resp = await http.PostAsJsonAsync(url, new
        {
            countryCode,
            phoneNumber,
            type
        });

        var raw = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer phone returned {(int)resp.StatusCode}: {raw}");

        // 2) Zeebe Complete (en uyumlu yöntem: JSON string ver)
        await client.NewCompleteJobCommand(job.Key)
            .Variables("{\"customerPhoneCreated\":true}")
            .Send();
    }

    // -------------------------
    // 6) customer.address.create (GERÇEK)
    // POST /api/v1/customers/{customerId}/addresses
    // -------------------------
    public async Task HandleCustomerAddressCreate(IJobClient client, IJob job)
    {
        // 1) Vars
        var customerId = GetString(job, "customerId");
        var addressType = GetString(job, "addressType");   // "Home" / "Work" / "1" gelebilir
        var city = GetString(job, "city");
        var district = GetString(job, "district");
        var postalCode = GetString(job, "postalCode");
        var addressLine1 = GetString(job, "addressLine1");

        // 2) Validation
        if (string.IsNullOrWhiteSpace(customerId))
            throw new InvalidOperationException("customerId missing (BPMN input mapping kontrol et)");

        if (string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(district) ||
            string.IsNullOrWhiteSpace(postalCode) ||
            string.IsNullOrWhiteSpace(addressLine1))
            throw new InvalidOperationException("Address fields missing (city/district/postalCode/addressLine1)");

        // 3) addressType -> int (Swagger format)
        // Swagger genelde 1=Home, 2=Work gibi.
        int addressTypeInt = MapAddressType(addressType);

        var body = new
        {
            addressType = addressTypeInt,
            city,
            district,
            postalCode,
            addressLine1
        };

        // 4) Call Customer API
        var http = _http.CreateClient("customer"); // base address: https://localhost:7101 vs
        var url = $"/api/v1/customers/{customerId}/addresses";

        using var resp = await http.PostAsync(
            url,
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        );

        var raw = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Customer address returned {(int)resp.StatusCode}: {raw}");

        // 5) Complete Job (DOĞRU YÖNTEM)
        var completeVarsJson = JsonSerializer.Serialize(new { customerAddressCreated = true });

        await client
            .NewCompleteJobCommand(job.Key)
            .Variables(completeVarsJson)
            .Send();
    }

    private static int MapAddressType(string? addressType)
    {
        if (string.IsNullOrWhiteSpace(addressType))
            return 1; // default Home

        // "1" / "2" geldiyse direkt
        if (int.TryParse(addressType, out var numeric))
            return numeric;

        // "Home" / "Work" geldiyse map
        return addressType.Trim().ToLowerInvariant() switch
        {
            "home" => 1,
            "work" => 2,
            _ => 1
        };
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