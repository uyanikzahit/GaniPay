using GaniPay.Accounting.Domain.Entities;

namespace GaniPay.Accounting.Application.Abstractions.Repositories;

public interface IAccountingTransactionRepository
{
    Task<AccountingTransaction?> GetByIdempotencyKeyAsync(Guid accountId, string idempotencyKey, CancellationToken ct);
    Task AddAsync(AccountingTransaction tx, CancellationToken ct);

    Task<decimal> CalculateUsageAsync(
        Guid customerId,
        string currency,
        string metricType,     // "TransactionCount" | "TransactionAmount"
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct);
}
