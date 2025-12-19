using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Dtos;

public sealed record CustomerLimitDto(
    Guid Id,
    Guid CustomerId,
    Guid LimitDefinitionId,
    short Year,
    short? Month,
    short? Day,
    decimal Value,
    string Currency,
    LimitSource Source,
    string? Reason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? UpdatedBy);
