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
        var accessToken = TryGetString(root, "accessToken"); // varsa auth ekleriz

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

            // Eğer Customer API authorize isterse diye:
            if (!string.IsNullOrWhiteSpace(accessToken))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // ✅ HTTP/1.1 zorla (ResponseEnded problemini çok kez çözer)
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

            if (!resp.IsSuccessStatusCode)
            {
                var raw = await resp.Content.ReadAsStringAsync();
                await CompleteAsync(client, job, new
                {
                    customerOk = false,
                    errorCode = $"CUSTOMER_HTTP_{(int)resp.StatusCode}",
                    customer = (object?)null,
                    customerError = raw
                });
                return;
            }

            // DTO parçalamıyoruz: response’u “customer” olarak aynen taşı
            using var bodyDoc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var customer = bodyDoc.RootElement.Clone();

            await CompleteAsync(client, job, new
            {
                customerOk = true,
                errorCode = (string?)null,
                customer
            });
        }
        catch (Exception ex)
        {
            // burada Complete yerine Fail de yapabilirsin ama MVP’de “customerOk=false” daha iyi
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
}
