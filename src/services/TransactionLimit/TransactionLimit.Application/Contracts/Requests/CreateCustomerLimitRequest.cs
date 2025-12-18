namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed record CreateCustomerLimitRequest(
    Guid CustomerId,
    Guid LimitDefinitionId,
    short? Year,
    short? Month,
    short? Day,
    decimal Value,
    string? Currency,
    string Source, // system/admin/migration (string)
    string? Reason
);
