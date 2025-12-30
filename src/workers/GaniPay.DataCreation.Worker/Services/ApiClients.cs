using System.Net.Http.Json;

namespace GaniPay.DataCreation.Worker.Services;

public sealed class ApiClients
{
    private readonly HttpClient _http;

    public ApiClients(HttpClient http)
    {
        _http = http;
    }

    // Þu an handler'larda mock üretiyoruz.
    // Ýleride gerçek servis çaðrýlarý için buraya metot ekleyeceksin.
}
