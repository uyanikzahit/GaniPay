using GaniPay.Customer.Application.Contracts.Enums;

namespace GaniPay.Customer.Application.Contracts.Dtos;

public sealed record CustomerDto(
    Guid Id,
    string CustomerNumber,
    CustomerType Type,
    CustomerSegment Segment,
    CustomerStatus Status,
    DateOnly OpenDate,
    DateOnly? CloseDate,
    string? CloseReason
);
