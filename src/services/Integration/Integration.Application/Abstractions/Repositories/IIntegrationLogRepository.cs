using GaniPay.Integration.Domain.Entities;

namespace GaniPay.Integration.Application.Abstractions.Repositories;

public interface IIntegrationLogRepository
{
    Task<IntegrationLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);

    Task AddAsync(IntegrationLog log, CancellationToken ct = default);
    Task UpdateAsync(IntegrationLog log, CancellationToken ct = default);
}
