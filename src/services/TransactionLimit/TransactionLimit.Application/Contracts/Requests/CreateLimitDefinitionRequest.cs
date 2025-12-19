using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed class CreateLimitDefinitionRequest
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public LimitPeriod Period { get; set; }
    public LimitMetricType MetricType { get; set; }
    public bool IsVisible { get; set; } = true;
}
