using GaniPay.Notification.Application.Abstractions.Providers;

namespace GaniPay.Notification.Infrastructure.Providers;

public sealed class MockNotificationProvider : INotificationProvider
{
    public Task SendAsync(string channel, string templateCode, string payload, CancellationToken ct = default)
    {
        // MVP mock: gerçek sms/email yok.
        // demo fail
        if (string.Equals(templateCode, "FORCE_FAIL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Mock provider forced failure.");

        return Task.CompletedTask;
    }
}
