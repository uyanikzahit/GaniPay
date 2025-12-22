namespace GaniPay.Payments.Application.Contracts.Requests;

public sealed record StartTopUpRequest(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string IdempotencyKey
);
