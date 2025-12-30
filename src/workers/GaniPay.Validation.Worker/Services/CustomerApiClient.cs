using System.Net.Http;

namespace GaniPay.Validation.Worker.Services;

public sealed class CustomerApiClient
{
    private readonly HttpClient _http;

    public CustomerApiClient(HttpClient http) => _http = http;

    public async Task<(bool ok, string body)> GetCustomerByIdAsync(Guid customerId, CancellationToken ct)
    {
        var res = await _http.GetAsync($"/api/v1/customers/{customerId}", ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        return (res.IsSuccessStatusCode, body);
    }
}
