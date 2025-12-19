using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Dtos;

public sealed record LimitDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    LimitPeriod Period,
    LimitMetricType MetricType,
    bool IsVisible);
