using System.Text.Json;
using GaniPay.Validation.Worker.Options;
using GaniPay.Validation.Worker.Services;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Validation.Worker.Handlers;

public sealed class ValidationHandlers
{
    private readonly ILogger<ValidationHandlers> _log;
    private readonly ValidationOptions _validationOptions;
    private readonly CustomerApiClient _customerApi;
    private readonly IntegrationApiClient _integrationApi;

    public ValidationHandlers(
        ILogger<ValidationHandlers> log,
        ValidationOptions validationOptions,
        CustomerApiClient customerApi,
        IntegrationApiClient integrationApi)
    {
        _log = log;
        _validationOptions = validationOptions;
        _customerApi = customerApi;
        _integrationApi = integrationApi;
    }

    public async Task HandleAgeControl(IJobClient client, IJob job)
    {
        var vars = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);

        var birthDateStr = vars?.GetValueOrDefault("birthDate")?.ToString(); // "1999-11-08"

        if (string.IsNullOrWhiteSpace(birthDateStr) || !DateOnly.TryParse(birthDateStr, out var birthDate))
        {
            var completeVars = new
            {
                ageControlOk = false,
                ageControlReason = "BIRTHDATE_INVALID"
            };

            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(completeVars))
                .Send();

            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow); // TR local istersen Now da olur
        var ok = today >= birthDate.AddYears(18);

        var resultVars = new
        {
            ageControlOk = ok,
            ageControlReason = ok ? "OK" : "AGE_UNDER_18"
        };

        _log.LogInformation("age.control birthDate={BirthDate} today={Today} ok={Ok}",
            birthDate, today, ok);

        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(resultVars))
            .Send();
    }
    // 2) mock.device.info.get
    public async Task HandleDeviceInfoGet(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            deviceInfo = new
            {
                ok = true,
                reason = "OK",
                deviceId = "mock-device-001",
                deviceModel = "MockPhone X",
                os = "Android",
                appVersion = "1.0.0"
            },
            validation = new { lastStep = "DeviceInfoGet" }
        };

        _log.LogInformation("[DeviceInfoGet] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 3) mock.customer.phone.get
    public async Task HandleCustomerPhoneGet(IJobClient client, IJob job)
    {
        var (vars, _) = Read(job);
        vars.TryGetGuid("customerId", out var customerId);

        var completeVars = new
        {
            customerPhone = new
            {
                ok = true,
                reason = "OK",
                customerId,
                phone = "+905555555555",
                isVerified = true
            },
            validation = new { lastStep = "CustomerPhoneGet" }
        };

        _log.LogInformation("[CustomerPhoneGet] ok=true customerId={CustomerId}", customerId);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 4) worker.customer.control
    public async Task HandleCustomerControl(IJobClient client, IJob job)
    {
        var (vars, _) = Read(job);

        var hasCustomerId = vars.TryGetGuid("customerId", out _);
        var hasEmail = vars.TryGetString("email", out var email) && !string.IsNullOrWhiteSpace(email);

        var ok = hasCustomerId || hasEmail;
        var reason = ok ? "OK" : "MISSING_CUSTOMER_IDENTIFIER";

        var completeVars = new
        {
            customerControl = new { ok, reason },
            validation = new { lastStep = "CustomerControl" }
        };

        _log.LogInformation("[CustomerControl] ok={Ok} reason={Reason}", ok, reason);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 5) mock.device.customer.link.check
    public async Task HandleDeviceCustomerLinkCheck(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            deviceCustomerLink = new
            {
                ok = true,
                reason = "OK",
                isLinked = true,
                matches = true
            },
            validation = new { lastStep = "DeviceCustomerLinkCheck" }
        };

        _log.LogInformation("[DeviceCustomerLinkCheck] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 6) mock.unlink.device
    public async Task HandleUnlinkDevice(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            unlinkDevice = new { ok = true, reason = "OK" },
            validation = new { lastStep = "UnlinkDevice" }
        };

        _log.LogInformation("[UnlinkDevice] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 7) real.customer.getById
    public async Task HandleCustomerGetById(IJobClient client, IJob job)
    {
        var (vars, _) = Read(job);

        if (!vars.TryGetGuid("customerId", out var customerId))
        {
            var failVars = new
            {
                customerGetById = new { ok = false, reason = "MISSING_customerId" },
                validation = new { lastStep = "CustomerGetById" }
            };

            _log.LogWarning("[CustomerGetById] missing customerId");
            await client.NewCompleteJobCommand(job.Key)
                .Variables(JsonSerializer.Serialize(failVars))
                .Send();
            return;
        }

        var (ok, body) = await _customerApi.GetCustomerByIdAsync(customerId, CancellationToken.None);
        var reason = ok ? "OK" : "CUSTOMER_API_NOT_FOUND_OR_ERROR";

        var completeVars = new
        {
            customerGetById = new { ok, reason, customerId, raw = body },
            validation = new { lastStep = "CustomerGetById" }
        };

        _log.LogInformation("[CustomerGetById] ok={Ok} customerId={CustomerId}", ok, customerId);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 8) mock.customer.getByEmail
    public async Task HandleCustomerGetByEmail(IJobClient client, IJob job)
    {
        var (vars, _) = Read(job);
        vars.TryGetString("email", out var email);

        var ok = !string.IsNullOrWhiteSpace(email);
        var reason = ok ? "OK" : "MISSING_email";

        var completeVars = new
        {
            customerGetByEmail = new { ok, reason, email, exists = ok ? false : (bool?)null },
            validation = new { lastStep = "CustomerGetByEmail" }
        };

        _log.LogInformation("[CustomerGetByEmail] ok={Ok} email={Email}", ok, email);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 9) mock.sanction.query
    public async Task HandleSanctionQuery(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            sanction = new { ok = true, reason = "OK", isSanctioned = false },
            validation = new { lastStep = "SanctionQuery" }
        };

        _log.LogInformation("[SanctionQuery] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 10) mock.citizen.verification
    public async Task HandleCitizenVerification(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            citizen = new { ok = true, reason = "OK", verified = true },
            validation = new { lastStep = "CitizenVerification" }
        };

        _log.LogInformation("[CitizenVerification] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 11) mock.iban.verification
    public async Task HandleIbanVerification(IJobClient client, IJob job)
    {
        var (vars, _) = Read(job);
        vars.TryGetString("iban", out var iban);

        var ok = !string.IsNullOrWhiteSpace(iban);
        var reason = ok ? "OK" : "MISSING_iban";

        var completeVars = new
        {
            iban = new { ok, reason, iban, verified = ok },
            validation = new { lastStep = "IbanVerification" }
        };

        _log.LogInformation("[IbanVerification] ok={Ok}", ok);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 12) real.address.verification
    public async Task HandleAddressVerification(IJobClient client, IJob job)
    {
        var (_, raw) = Read(job);

        // Integration endpoint'inin kesin sözleşmesini bilmediğimiz için esnek payload
        var payload = new
        {
            provider = "AddressVerification",
            action = "verify",
            request = JsonSerializer.Deserialize<object>(raw)
        };

        var (ok, body) = await _integrationApi.CallAsync(payload, CancellationToken.None);
        var reason = ok ? "OK" : "INTEGRATION_ADDRESS_VERIFY_FAILED";

        var completeVars = new
        {
            address = new { ok, reason, raw = body },
            validation = new { lastStep = "AddressVerification" }
        };

        _log.LogInformation("[AddressVerification] ok={Ok}", ok);
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    // 13) mock.sim.check.control
    public async Task HandleSimCheckControl(IJobClient client, IJob job)
    {
        var completeVars = new
        {
            sim = new { ok = true, reason = "OK", matches = true },
            validation = new { lastStep = "SimCheckControl" }
        };

        _log.LogInformation("[SimCheckControl] ok=true");
        await client.NewCompleteJobCommand(job.Key)
            .Variables(JsonSerializer.Serialize(completeVars))
            .Send();
    }

    private static (JobVariables vars, string rawJson) Read(IJob job)
    {
        var raw = job.Variables ?? "{}";
        using var doc = JsonDocument.Parse(raw);
        return (new JobVariables(doc.RootElement), raw);
    }
}
