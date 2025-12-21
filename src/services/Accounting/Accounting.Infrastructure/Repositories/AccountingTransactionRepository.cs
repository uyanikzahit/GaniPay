using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountingTransactionRepository : IAccountingTransactionRepository
{
    private readonly AccountingDbContext _db;

    public AccountingTransactionRepository(AccountingDbContext db) => _db = db;

    public Task<AccountingTransaction?> GetByIdempotencyKeyAsync(Guid accountId, string idempotencyKey, CancellationToken ct)
        => _db.AccountingTransactions.FirstOrDefaultAsync(
            x => x.AccountId == accountId && x.IdempotencyKey == idempotencyKey, ct);

    public Task AddAsync(AccountingTransaction tx, CancellationToken ct)
        => _db.AccountingTransactions.AddAsync(tx, ct).AsTask();

    public async Task<decimal> CalculateUsageAsync(
        Guid customerId,
        string currency,
        string metricType,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        // Account join
        var query =
            from tx in _db.AccountingTransactions.AsNoTracking()
            join acc in _db.Accounts.AsNoTracking() on tx.AccountId equals acc.Id
            where acc.CustomerId == customerId
                  && acc.Currency == currency
                  && tx.Status == Domain.Enums.TransactionStatus.Booked
                  && tx.CreatedAt >= fromUtc && tx.CreatedAt <= toUtc
            select tx;

        if (string.Equals(metricType, "TransactionAmount", StringComparison.OrdinalIgnoreCase))
        {
            return await query.SumAsync(x => x.Amount, ct);
        }

        // default: TransactionCount
        return await query.CountAsync(ct);
    }
}
