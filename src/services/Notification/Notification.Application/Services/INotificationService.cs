using GaniPay.Notification.Application.Contracts.Dtos;
using GaniPay.Notification.Application.Contracts.Requests;

namespace GaniPay.Notification.Application.Services;

public interface INotificationService
{
    Task<SendNotificationResultDto> SendAsync(SendNotificationRequest request, CancellationToken ct);
    Task<NotificationLogDto> GetAsync(GetNotificationRequest request, CancellationToken ct);
    Task<List<NotificationLogDto>> GetCustomerLogsAsync(GetCustomerNotificationsRequest request, CancellationToken ct);
}
