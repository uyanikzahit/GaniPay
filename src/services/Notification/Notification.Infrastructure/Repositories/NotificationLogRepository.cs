using GaniPay.Notification.Application.Abstractions.Repositories;
using GaniPay.Notification.Domain.Entities;
using GaniPay.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Notification.Infrastructure.Repositories;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _db;

    public NotificationLogRepository(NotificationDbContext db) => _db = db;

    public Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.NotificationLogs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<NotificationLog>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => _db.NotificationLogs
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task AddAsync(NotificationLog log, CancellationToken ct = default)
        => _db.NotificationLogs.AddAsync(log, ct).AsTask();

    public Task UpdateAsync(NotificationLog log, CancellationToken ct = default)
    {
        _db.NotificationLogs.Update(log);
        return Task.CompletedTask;
    }
}
