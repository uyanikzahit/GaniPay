namespace GaniPay.Notification.Application.Contracts.Dtos;

public sealed record SendNotificationResultDto(
    Guid NotificationId,
    string Status
);
