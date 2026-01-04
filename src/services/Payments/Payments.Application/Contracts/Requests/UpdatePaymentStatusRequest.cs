namespace GaniPay.Payments.Application.Contracts.Requests;

public sealed record UpdatePaymentStatusRequest(
    string CorrelationId,
    string Status,          // "Succeeded" | "Failed" (istersen enum yap)
    string? ErrorCode = null,
    string? ErrorMessage = null
);