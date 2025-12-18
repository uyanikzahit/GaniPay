namespace GaniPay.TransactionLimit.Application.Contracts.Dtos;

public sealed record LimitCheckResultDto(
    bool Allowed,
    string Reason,
    decimal? Limit,
    decimal Requested
);
