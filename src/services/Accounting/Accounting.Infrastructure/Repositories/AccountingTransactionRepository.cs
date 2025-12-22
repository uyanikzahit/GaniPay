using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountingTransactionRepository : IAccountingTransactionRepository
{
    private readonly AccountingDbContext _db;
    public AccountingTransactionRepository(AccountingDbContext db) => _db = db;

    public async Task AddAsync(AccountingTransaction tx, CancellationToken ct = default)
        => await _db.AccountingTransactions.AddAsync(tx, ct);

    public async Task<IReadOnlyList<AccountingTransaction>> ListByAccountAndRangeAsync(
        Guid accountId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        return await _db.AccountingTransactions.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.CreatedAt >= fromUtc && x.CreatedAt <= toUtc)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AccountingTransaction>> ListByCustomerAndCurrencyAndRangeAsync(
        Guid customerId,
        string currency,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        return await _db.AccountingTransactions.AsNoTracking()
            .Where(x =>
                x.Account.CustomerId == customerId &&
                x.Currency == currency &&
                x.CreatedAt >= fromUtc &&
                x.CreatedAt <= toUtc)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
