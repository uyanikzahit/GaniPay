using GaniPay.Login.Worker.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace GaniPay.Login.Worker.Handlers;

public sealed class CustomerGetJobHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CustomerApiOptions _opt;

    public CustomerGetJobHandler(IHttpClientFactory httpClientFactory, IOptions<CustomerApiOptions> opt)
    {
        _httpClientFactory = httpClientFactory;
        _opt = opt.Value;
    }

    public async Task Handle(IJobClient client, IJob job)
    {
        using var doc = JsonDocument.Parse(job.Variables);
        var root = doc.RootElement;

        var customerId = TryGetString(root, "customerId");
        var accessToken = TryGetString(root, "accessToken");

        if (string.IsNullOrWhiteSpace(customerId))
        {
            await CompleteAsync(client, job, new
            {
                customerOk = false,
                errorCode = "CUSTOMER_INVALID_INPUT",
                customer = (object?)null
            });
            return;
        }

        try
        {
            var baseUrl = (_opt.BaseUrl ?? "http://host.docker.internal:7101").TrimEnd('/');
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(baseUrl);
            http.Timeout = TimeSpan.FromSeconds(15);

            if (!string.IsNullOrWhiteSpace(accessToken))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // ✅ HTTP/1.1 zorla (ResponseEnded problemini engeller)
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/customers/{customerId}")
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };

            var resp = await http.SendAsync(req);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                await CompleteAsync(client, job, new
                {
                    customerOk = false,
                    errorCode = "CUSTOMER_NOT_FOUND",
                    customer = (object?)null
                });
                return;
            }

            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                await CompleteAsync(client, job, new
                {
                    customerOk = false,
                    errorCode = $"CUSTOMER_HTTP_{(int)resp.StatusCode}",
                    customer = (object?)null,
                    customerError = raw
                });
                return;
            }

            // ✅ DTO yok: raw json güvenli parse
            using var body = JsonDocument.Parse(raw);
            var b = body.RootElement;

            // Üst seviye alanlar (varsa)
            var id = GetString(b, "id") ?? customerId;
            var customerNumber = GetString(b, "customerNumber");
            var status = GetString(b, "status");
            var segment = GetString(b, "segment");
            var type = GetString(b, "type");

            // ✅ İSİMLER individual içinde geliyor (Customer API response böyle)
            var individual = GetObject(b, "individual");

            var firstName =
                (individual is null ? null : GetString(individual.Value, "firstName"))
                ?? GetString(b, "firstName"); // fallback (olur da root'a taşınırsa)

            var lastName =
                (individual is null ? null : GetString(individual.Value, "lastName"))
                ?? GetString(b, "lastName");

            var birthDate =
                (individual is null ? null : GetString(individual.Value, "birthDate"))
                ?? GetString(b, "birthDate");

            var nationality =
                (individual is null ? null : GetString(individual.Value, "nationality"))
                ?? GetString(b, "nationality");

            // emails[0].emailAddress
            var email = GetFirstArrayObjectString(b, "emails", "emailAddress");

            // phones[0].phoneNumber
            var phoneNumber = GetFirstArrayObjectString(b, "phones", "phoneNumber");

            // addresses[0] basic
            var addrObj = GetFirstArrayObject(b, "addresses");
            var address = addrObj is null
                ? null
                : new
                {
                    addressType = GetString(addrObj.Value, "addressType"),
                    city = GetString(addrObj.Value, "city"),
                    district = GetString(addrObj.Value, "district"),
                    postalCode = GetString(addrObj.Value, "postalCode"),
                    addressLine1 = GetString(addrObj.Value, "addressLine1")
                };

            // ✅ Camunda’da kullanacağımız customer objesi (küçük ve sabit)
            var customerOut = new
            {
                customerId = id,
                customerNumber,
                status,
                segment,
                type,
                firstName,
                lastName,
                birthDate,
                nationality,
                email,
                phoneNumber,
                address
            };

            await CompleteAsync(client, job, new
            {
                customerOk = true,
                errorCode = (string?)null,
                customer = customerOut
            });
        }
        catch (Exception ex)
        {
            await CompleteAsync(client, job, new
            {
                customerOk = false,
                errorCode = "CUSTOMER_EXCEPTION",
                customer = (object?)null,
                customerError = ex.Message
            });
        }
    }

    private static async Task CompleteAsync(IJobClient client, IJob job, object variables)
    {
        var json = JsonSerializer.Serialize(variables, JsonOpts);
        await client.NewCompleteJobCommand(job.Key).Variables(json).Send();
    }

    private static string? TryGetString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;
        return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
    }

    private static JsonElement? GetObject(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;
        return p.ValueKind == JsonValueKind.Object ? p : null;
    }

    private static string? GetString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var p)) return null;
        if (p.ValueKind == JsonValueKind.String) return p.GetString();
        if (p.ValueKind == JsonValueKind.Number) return p.GetRawText();
        if (p.ValueKind == JsonValueKind.True) return "true";
        if (p.ValueKind == JsonValueKind.False) return "false";
        return null;
    }

    private static JsonElement? GetFirstArrayObject(JsonElement root, string arrayName)
    {
        if (!root.TryGetProperty(arrayName, out var arr)) return null;
        if (arr.ValueKind != JsonValueKind.Array) return null;
        using var e = arr.EnumerateArray();
        if (!e.MoveNext()) return null;
        return e.Current.ValueKind == JsonValueKind.Object ? e.Current : null;
    }

    private static string? GetFirstArrayObjectString(JsonElement root, string arrayName, string fieldName)
    {
        var obj = GetFirstArrayObject(root, arrayName);
        return obj is null ? null : GetString(obj.Value, fieldName);
    }
}
