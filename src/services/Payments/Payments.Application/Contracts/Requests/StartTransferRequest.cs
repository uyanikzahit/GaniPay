using GaniPay.Payments.Application.Contracts.Enums;

namespace GaniPay.Payments.Application.Contracts.Requests;

public sealed record StartTransferRequest(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string TargetWalletNumber,
    string IdempotencyKey,
    TransferType TransferType = TransferType.Unknown
);