using GaniPay.Integration.Application.Abstractions.Providers;

namespace GaniPay.Integration.Infrastructure.Providers;

public sealed class MockBankProvider : IIntegrationProviderClient
{
    public Task<string> CallAsync(string operation, string requestPayload, CancellationToken ct = default)
    {
        if (requestPayload.Contains("FORCE_FAIL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Mock bank forced failure.");

        var providerRef = Guid.NewGuid().ToString("N");
        var responseJson = $"{{\"providerRef\":\"{providerRef}\",\"operation\":\"{operation}\",\"result\":\"OK\"}}";
        return Task.FromResult(responseJson);
    }
}
