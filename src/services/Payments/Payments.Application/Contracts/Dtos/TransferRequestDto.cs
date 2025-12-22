using GaniPay.Payments.Application.Contracts.Enums;

namespace GaniPay.Payments.Application.Contracts.Dtos;

public sealed record TransferRequestDto(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string TargetIban,
    string IdempotencyKey,
    TransferType TransferType = TransferType.Unknown
);
