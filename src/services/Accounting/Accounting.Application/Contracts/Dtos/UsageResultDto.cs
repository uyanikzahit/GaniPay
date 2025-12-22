namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record UsageResultDto(
    Guid CustomerId,
    string Currency,
    string Period,
    string MetricType,
    DateOnly From,
    DateOnly To,
    decimal UsedAmount,
    int UsedCount
);
