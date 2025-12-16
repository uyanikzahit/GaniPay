using GaniPay.Identity.Domain.Entities;

namespace GaniPay.Identity.Infrastructure.Repositories.Abstractions;

public interface ICredentialRecoveryRepository
{
    Task<CredentialRecovery?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CredentialRecovery entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
