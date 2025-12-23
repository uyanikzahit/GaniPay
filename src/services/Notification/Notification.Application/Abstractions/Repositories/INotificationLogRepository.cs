using GaniPay.Notification.Domain.Entities;

namespace GaniPay.Notification.Application.Abstractions.Repositories;

public interface INotificationLogRepository
{
    Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<NotificationLog>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    Task AddAsync(NotificationLog log, CancellationToken ct = default);
    Task UpdateAsync(NotificationLog log, CancellationToken ct = default);
}
