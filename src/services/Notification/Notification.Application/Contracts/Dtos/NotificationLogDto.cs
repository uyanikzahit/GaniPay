namespace GaniPay.Notification.Application.Contracts.Dtos;

public sealed record NotificationLogDto(
    Guid Id,
    Guid CustomerId,
    string Channel,
    string TemplateCode,
    string Payload,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt,
    string? ErrorMessage
);
