using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed class UsageQueryRequest
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = "TRY";

    public UsageMetricType MetricType { get; set; }
    public UsagePeriod Period { get; set; }
}
