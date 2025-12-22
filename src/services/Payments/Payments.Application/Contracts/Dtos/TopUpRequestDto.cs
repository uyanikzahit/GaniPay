namespace GaniPay.Payments.Application.Contracts.Dtos;

public sealed record TopUpRequestDto(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string IdempotencyKey
);
