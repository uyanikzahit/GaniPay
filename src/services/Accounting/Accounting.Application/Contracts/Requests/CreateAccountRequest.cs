namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed record CreateAccountRequest(
    Guid CustomerId,
    string Currency,
    string? Iban
);