namespace GaniPay.Payments.Application.Contracts.Dtos;

public sealed record StartPaymentResultDto(
    string CorrelationId,
    string Status
);
