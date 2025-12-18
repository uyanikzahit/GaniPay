using GaniPay.TransactionLimit.Application.Contracts.Enums;

namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed record CreateCustomerLimitRequest(
    Guid CustomerId,
    Guid LimitDefinitionId,
    short? Year,
    short? Month,
    short? Day,
    decimal Value,
    string Currency,
    LimitSource Source,
    string? Reason,
    string? UpdatedBy
);
