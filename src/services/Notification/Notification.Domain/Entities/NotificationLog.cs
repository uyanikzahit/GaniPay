using GaniPay.Notification.Domain.Enums;

namespace GaniPay.Notification.Domain.Entities;

public sealed class NotificationLog
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    public string Channel { get; set; } = default!;
    public string TemplateCode { get; set; } = default!;
    public string Payload { get; set; } = default!;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }
}
