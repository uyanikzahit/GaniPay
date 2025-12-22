namespace GaniPay.Payments.Application.Contracts.Requests;

public sealed record GetPaymentStatusRequest(
    string CorrelationId
);
