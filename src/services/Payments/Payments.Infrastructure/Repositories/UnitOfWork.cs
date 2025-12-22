using GaniPay.Payments.Application.Abstractions;
using GaniPay.Payments.Infrastructure.Persistence;

namespace GaniPay.Payments.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _db;

    public UnitOfWork(PaymentsDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
