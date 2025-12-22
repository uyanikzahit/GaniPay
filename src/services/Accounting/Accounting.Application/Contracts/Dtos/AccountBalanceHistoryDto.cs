namespace GaniPay.Accounting.Application.Contracts.Dtos;

public sealed record AccountBalanceHistoryDto(
    Guid Id,
    Guid AccountId,
    string Direction,
    decimal ChangeAmount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string Currency,
    int OperationType,
    Guid ReferenceId,
    DateTime CreatedAt
);
