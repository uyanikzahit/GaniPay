using GaniPay.Integration.Application.Abstractions.Repositories;
using GaniPay.Integration.Domain.Entities;
using GaniPay.Integration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Integration.Infrastructure.Repositories;

public sealed class IntegrationProviderRepository : IIntegrationProviderRepository
{
    private readonly IntegrationDbContext _db;

    public IntegrationProviderRepository(IntegrationDbContext db) => _db = db;

    public Task<IntegrationProvider?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.IntegrationProviders.FirstOrDefaultAsync(x => x.Code == code, ct);

    public Task<IntegrationProvider?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.IntegrationProviders.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(IntegrationProvider provider, CancellationToken ct = default)
        => _db.IntegrationProviders.AddAsync(provider, ct).AsTask();

    public Task UpdateAsync(IntegrationProvider provider, CancellationToken ct = default)
    {
        _db.IntegrationProviders.Update(provider);
        return Task.CompletedTask;
    }
}
