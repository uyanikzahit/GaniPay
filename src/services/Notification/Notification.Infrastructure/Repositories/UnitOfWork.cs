using GaniPay.Notification.Application.Abstractions;
using GaniPay.Notification.Infrastructure.Persistence;

namespace GaniPay.Notification.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NotificationDbContext _db;

    public UnitOfWork(NotificationDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
