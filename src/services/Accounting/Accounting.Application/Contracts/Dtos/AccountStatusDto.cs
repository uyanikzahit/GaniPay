namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record AccountStatusDto(
    Guid AccountId,
    Guid CustomerId,
    string Currency,
    int Status
);