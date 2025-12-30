using System.Net.Http;
using System.Net.Http.Json;

namespace GaniPay.Validation.Worker.Services;

public sealed class IntegrationApiClient
{
    private readonly HttpClient _http;

    public IntegrationApiClient(HttpClient http) => _http = http;

    public async Task<(bool ok, string body)> CallAsync(object payload, CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync("/api/integration/call", payload, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        return (res.IsSuccessStatusCode, body);
    }
}
