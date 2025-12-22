namespace GaniPay.Payments.Application.Contracts.Dtos;

public sealed record PaymentProcessDto(
    Guid Id,
    string CorrelationId,
    Guid CustomerId,
    string Type,
    string Status,
    decimal Amount,
    string Currency,
    long? WorkflowInstanceKey,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
