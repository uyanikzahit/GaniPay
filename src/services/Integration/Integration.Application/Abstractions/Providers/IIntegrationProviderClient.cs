namespace GaniPay.Integration.Application.Abstractions.Providers;

public interface IIntegrationProviderClient
{
    Task<string> CallAsync(string operation, string requestPayload, CancellationToken ct = default);
}
