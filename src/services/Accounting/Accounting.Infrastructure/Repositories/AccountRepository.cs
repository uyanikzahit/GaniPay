using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Domain.Entities;
using GaniPay.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountingDbContext _db;

    public AccountRepository(AccountingDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid customerId, string currency, CancellationToken ct)
        => _db.Accounts.AnyAsync(x => x.CustomerId == customerId && x.Currency == currency, ct);

    public Task<Account?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct)
        => _db.Accounts.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Currency == currency, ct);

    public Task AddAsync(Account account, CancellationToken ct)
        => _db.Accounts.AddAsync(account, ct).AsTask();

    public Task UpdateAsync(Account account, CancellationToken ct)
    {
        _db.Accounts.Update(account);
        return Task.CompletedTask;
    }
}
