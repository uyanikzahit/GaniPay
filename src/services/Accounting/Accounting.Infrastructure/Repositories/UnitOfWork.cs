using GaniPay.Accounting.Application.Abstractions;
using GaniPay.Accounting.Infrastructure.Persistence;

namespace GaniPay.Accounting.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AccountingDbContext _db;
    public UnitOfWork(AccountingDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
