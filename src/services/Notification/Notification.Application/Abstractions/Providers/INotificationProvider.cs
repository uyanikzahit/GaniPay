namespace GaniPay.Notification.Application.Abstractions.Providers;

public interface INotificationProvider
{
    Task SendAsync(string channel, string templateCode, string payload, CancellationToken ct = default);
}
