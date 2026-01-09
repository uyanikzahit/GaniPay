using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountingDbContext _db;
    public AccountRepository(AccountingDbContext db) => _db = db;

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default)
        => _db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, ct);

    //public Task<Account?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct = default)
    //    => _db.Accounts.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Currency == currency, ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
        => await _db.Accounts.AddAsync(account, ct);

    public Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        _db.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public Task<Account?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct)
    {
        var ccy = currency.Trim().ToUpperInvariant();

        return _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Currency == ccy, ct);
    }

    public async Task<List<Account>> ListByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
        return await _db.Accounts
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
