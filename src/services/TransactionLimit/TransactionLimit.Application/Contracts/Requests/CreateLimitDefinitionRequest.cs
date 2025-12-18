using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed record CreateLimitDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    LimitPeriod Period,
    LimitMetricType MetricType,
    bool IsVisible
);
