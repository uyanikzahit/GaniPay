namespace GaniPay.TransactionLimit.Application.Contracts.Dtos;

public sealed record LimitCheckResultDto(
    bool Allowed,
    string Message,
    decimal RequestedValue,
    decimal? LimitValue);
