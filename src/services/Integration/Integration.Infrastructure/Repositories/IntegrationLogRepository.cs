using GaniPay.Integration.Application.Abstractions.Repositories;
using GaniPay.Integration.Domain.Entities;
using GaniPay.Integration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Integration.Infrastructure.Repositories;

public sealed class IntegrationLogRepository : IIntegrationLogRepository
{
    private readonly IntegrationDbContext _db;

    public IntegrationLogRepository(IntegrationDbContext db) => _db = db;

    public Task<IntegrationLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.IntegrationLogs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<IntegrationLog>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
        => _db.IntegrationLogs
            .Where(x => x.ProviderId == providerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task AddAsync(IntegrationLog log, CancellationToken ct = default)
        => _db.IntegrationLogs.AddAsync(log, ct).AsTask();

    public Task UpdateAsync(IntegrationLog log, CancellationToken ct = default)
    {
        _db.IntegrationLogs.Update(log);
        return Task.CompletedTask;
    }
}
