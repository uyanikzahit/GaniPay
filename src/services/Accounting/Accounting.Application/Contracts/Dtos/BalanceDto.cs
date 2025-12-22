namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record BalanceDto(
    Guid AccountId,
    Guid CustomerId,
    string Currency,
    decimal Balance
);
