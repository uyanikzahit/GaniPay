using Microsoft.EntityFrameworkCore;
using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Infrastructure.Persistence;
using DomainAccount = GaniPay.Accounting.Domain.Entities.Account;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountingDbContext _db;

    public AccountRepository(AccountingDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid customerId, string currency, CancellationToken ct)
        => _db.Accounts.AnyAsync(x => x.CustomerId == customerId && x.Currency == currency, ct);

    public Task<DomainAccount?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct)
        => _db.Accounts.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Currency == currency, ct);

    public async Task AddAsync(DomainAccount account, CancellationToken ct)
        => await _db.Accounts.AddAsync(account, ct);

    public Task UpdateAsync(DomainAccount account, CancellationToken ct)
    {
        _db.Accounts.Update(account);
        return Task.CompletedTask;
    }
}
