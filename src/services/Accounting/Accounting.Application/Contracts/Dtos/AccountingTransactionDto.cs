namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record AccountingTransactionDto(
    Guid Id,
    Guid AccountId,
    string Direction,
    decimal Amount,
    string Currency,
    decimal BalanceBefore,
    decimal BalanceAfter,
    int OperationType,
    Guid ReferenceId,
    DateTime CreatedAt
);
