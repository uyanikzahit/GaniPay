using GaniPay.TransactionLimit.Domain.Enums;

namespace GaniPay.TransactionLimit.Domain.Entities;

public sealed class LimitDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public LimitPeriod Period { get; set; }
    public LimitMetricType MetricType { get; set; }

    public bool IsVisible { get; set; } = true;
}
