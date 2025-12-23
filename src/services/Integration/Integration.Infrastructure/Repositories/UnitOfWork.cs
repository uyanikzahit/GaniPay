using GaniPay.Integration.Application.Abstractions;
using GaniPay.Integration.Infrastructure.Persistence;

namespace GaniPay.Integration.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IntegrationDbContext _db;

    public UnitOfWork(IntegrationDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
