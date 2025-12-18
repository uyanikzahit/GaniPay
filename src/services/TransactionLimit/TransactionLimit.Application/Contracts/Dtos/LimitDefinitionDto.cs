namespace GaniPay.TransactionLimit.Application.Contracts.Dtos;

public sealed record LimitDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Period,
    string MetricType,
    bool IsVisible
);
