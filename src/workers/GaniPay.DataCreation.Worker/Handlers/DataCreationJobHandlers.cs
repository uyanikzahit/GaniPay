using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.DataCreation.Worker.Handlers;

public sealed class DataCreationJobHandlers
{
    private readonly ILogger<DataCreationJobHandlers> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public DataCreationJobHandlers(
        ILogger<DataCreationJobHandlers> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

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

    private static void Complete(IJobClient client, IJob job, Dictionary<string, object> variables)
    {
        var json = JsonSerializer.Serialize(variables, JsonOpts);

        client.NewCompleteJobCommand(job.Key)
            .Variables(json)
            .Send()
            .GetAwaiter()
            .GetResult();
    }

    // ✅ SADECE EKLENEN: job.Variables içinden string okuma (VariablesAsDictionary YOK!)
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

    // ✅ SADECE EKLENEN: response json içinden guid çekme
    private static Guid ExtractGuid(string json, string prop)
    {
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty(prop, out var p))
            throw new InvalidOperationException($"Response does not contain '{prop}'. Raw: {json}");

        if (p.ValueKind == JsonValueKind.String)
            return Guid.Parse(p.GetString()!);

        return p.GetGuid();
    }

    // -------------------------
    // Job Handlers (void)
    // -------------------------

    public void HandleWalletAccountLedgerIdGet(IJobClient client, IJob job)
    {
        _logger.LogInformation("[accounting.wallet.ledgerid.get] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["ledgerId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[accounting.wallet.ledgerid.get] COMPLETED key={Key} ledgerId={LedgerId}",
                job.Key, vars["ledgerId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[accounting.wallet.ledgerid.get] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[customer.create] COMPLETED key={Key} customerId={CustomerId}",
                job.Key, vars["customerId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[customer.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerIndividualCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.individual.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerIndividualId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[customer.individual.create] COMPLETED key={Key} id={Id}",
                job.Key, vars["customerIndividualId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[customer.individual.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerPhoneCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.phone.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerPhoneId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[customer.phone.create] COMPLETED key={Key} id={Id}",
                job.Key, vars["customerPhoneId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[customer.phone.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleLinkDeviceAndCustomer(IJobClient client, IJob job)
    {
        _logger.LogInformation("[mock.device.customer.link] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["deviceCustomerLinkId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[mock.device.customer.link] COMPLETED key={Key} id={Id}",
                job.Key, vars["deviceCustomerLinkId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[mock.device.customer.link] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleAccountCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[accounting.account.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["accountId"] = Guid.NewGuid().ToString(),
                ["accountNumber"] = $"AC{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            };

            Complete(client, job, vars);

            _logger.LogInformation("[accounting.account.create] COMPLETED key={Key} accountId={AccountId}",
                job.Key, vars["accountId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[accounting.account.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerEmailCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.email.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerEmailId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[customer.email.create] COMPLETED key={Key} id={Id}",
                job.Key, vars["customerEmailId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[customer.email.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerAddressCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[customer.address.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerAddressId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[customer.address.create] COMPLETED key={Key} id={Id}",
                job.Key, vars["customerAddressId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[customer.address.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    public void HandleCustomerOccupationCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[mock.customer.occupation.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            var vars = new Dictionary<string, object>
            {
                ["customerOccupationId"] = Guid.NewGuid().ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[mock.customer.occupation.create] COMPLETED key={Key} id={Id}",
                job.Key, vars["customerOccupationId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[mock.customer.occupation.create] ERROR key={Key}", job.Key);
            throw;
        }
    }

    // -------------------------
    // ✅ SADECE EKLENEN: Identity Credential Create
    // JobType: identity.credential.create
    // -------------------------
    public void HandleIdentityCredentialCreate(IJobClient client, IJob job)
    {
        _logger.LogInformation("[identity.credential.create] RECEIVED key={Key} retries={Retries} vars={Vars}",
            job.Key, job.Retries, Vars(job));

        try
        {
            // BPMN variables (register request’ten geliyor olmalı)
            var customerIdStr = GetString(job, "customerId");
            var phoneNumber = GetString(job, "phoneNumber");
            var password = GetString(job, "password");

            if (string.IsNullOrWhiteSpace(customerIdStr))
                throw new InvalidOperationException("customerId variable is missing");
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("phoneNumber variable is missing");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("password variable is missing");

            var body = new
            {
                customerId = Guid.Parse(customerIdStr),
                phoneNumber,
                password
            };

            var identityClient = _httpClientFactory.CreateClient("identity");

            var resp = identityClient
                .PostAsJsonAsync("/api/v1/identity/registrations/start", body)
                .GetAwaiter()
                .GetResult();

            var raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("[identity.credential.create] FAILED key={Key} status={Status} body={Body}",
                    job.Key, (int)resp.StatusCode, raw);
                throw new InvalidOperationException($"Identity returned {(int)resp.StatusCode}: {raw}");
            }

            // Identity response: { "id": "...", "customerId": "..." } bekliyoruz
            var credentialId = ExtractGuid(raw, "id");

            var vars = new Dictionary<string, object>
            {
                ["credentialId"] = credentialId.ToString()
            };

            Complete(client, job, vars);

            _logger.LogInformation("[identity.credential.create] COMPLETED key={Key} credentialId={CredentialId}",
                job.Key, credentialId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[identity.credential.create] ERROR key={Key}", job.Key);
            throw;
        }
    }
}
