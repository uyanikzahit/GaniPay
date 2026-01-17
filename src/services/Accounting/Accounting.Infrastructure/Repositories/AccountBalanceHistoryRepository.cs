using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountBalanceHistoryRepository : IAccountBalanceHistoryRepository
{
    private readonly AccountingDbContext _db;
    public AccountBalanceHistoryRepository(AccountingDbContext db) => _db = db;

    public async Task AddAsync(AccountBalanceHistory history, CancellationToken ct = default)
        => await _db.AccountBalanceHistories.AddAsync(history, ct);

    public async Task<IReadOnlyList<AccountBalanceHistory>> ListByAccountAndRangeAsync(
        Guid accountId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        return await _db.AccountBalanceHistories.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.CreatedAt >= fromUtc && x.CreatedAt <= toUtc)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }


    public async Task<IReadOnlyList<AccountBalanceHistory>> ListByAccountIdAsync(
    Guid accountId,
    CancellationToken ct = default)
    {
        return await _db.AccountBalanceHistories.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAt) // en yeni üstte (istersen OrderBy yaparýz)
            .ToListAsync(ct);
    }
}
