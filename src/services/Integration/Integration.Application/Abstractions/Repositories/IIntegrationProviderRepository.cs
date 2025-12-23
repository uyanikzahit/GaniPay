using GaniPay.Integration.Domain.Entities;

namespace GaniPay.Integration.Application.Abstractions.Repositories;

public interface IIntegrationProviderRepository
{
    Task<IntegrationProvider?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IntegrationProvider?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(IntegrationProvider provider, CancellationToken ct = default);
    Task UpdateAsync(IntegrationProvider provider, CancellationToken ct = default);
}
