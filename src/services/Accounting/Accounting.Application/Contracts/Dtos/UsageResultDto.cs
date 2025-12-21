using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed class UsageResultDto
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = default!;
    public UsageMetricType MetricType { get; set; }
    public UsagePeriod Period { get; set; }

    public decimal Value { get; set; }

    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
}
