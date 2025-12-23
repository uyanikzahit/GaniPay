namespace GaniPay.Notification.Application.Contracts.Requests;

public sealed record SendNotificationRequest(
    Guid CustomerId,
    string Channel,
    string TemplateCode,
    string Payload
);
