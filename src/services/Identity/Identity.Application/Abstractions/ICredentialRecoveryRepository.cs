using GaniPay.Identity.Domain.Entities;

namespace GaniPay.Identity.Application.Abstractions;

public interface ICredentialRecoveryRepository
{
    Task<CredentialRecovery?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task AddAsync(CredentialRecovery entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
