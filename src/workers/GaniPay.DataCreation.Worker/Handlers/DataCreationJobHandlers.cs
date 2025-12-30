using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.DataCreation.Worker.Handlers;

public sealed class DataCreationJobHandlers
{
    private readonly ILogger<DataCreationJobHandlers> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public DataCreationJobHandlers(ILogger<DataCreationJobHandlers> logger)
    {
        _logger = logger;
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
}
