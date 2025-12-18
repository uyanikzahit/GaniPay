namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed record LimitCheckRequest(
    Guid CustomerId,
    string Code,
    decimal Amount
);
