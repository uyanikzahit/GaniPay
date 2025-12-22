namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record AccountDto(
    Guid Id,
    Guid CustomerId,
    string AccountNumber,
    string Currency,
    decimal Balance,
    int Status,
    string? Iban,
    DateTime CreatedAt
);
