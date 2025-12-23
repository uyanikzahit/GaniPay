using GaniPay.Integration.Domain.Enums;

namespace GaniPay.Integration.Domain.Entities;

public sealed class IntegrationLog
{
    public Guid Id { get; set; }

    public Guid ProviderId { get; set; }              // FK -> integration_provider.id
    public string Operation { get; set; } = default!; // topup / transfer / notification_send

    public string RequestPayload { get; set; } = default!;  // json string
    public string ResponsePayload { get; set; } = default!; // json string

    public IntegrationStatus Status { get; set; } = IntegrationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}