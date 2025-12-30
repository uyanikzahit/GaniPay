using System.Net.Http.Headers;

namespace GaniPay.DataCreation.Worker.Services;

public sealed class ApiClients
{
    public HttpClient Customer { get; }
    public HttpClient Accounting { get; }

    // ✅ EKLE
    public HttpClient Identity { get; }

    public ApiClients(IHttpClientFactory factory)
    {
        Customer = factory.CreateClient("customer");
        Accounting = factory.CreateClient("accounting");

        // ✅ EKLE
        Identity = factory.CreateClient("identity");
    }
}