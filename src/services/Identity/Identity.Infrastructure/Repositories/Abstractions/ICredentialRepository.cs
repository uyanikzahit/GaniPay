using GaniPay.Identity.Domain.Entities;

namespace GaniPay.Identity.Infrastructure.Repositories.Abstractions;

public interface ICredentialRepository
{
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Credential?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task AddAsync(Credential entity, CancellationToken ct = default);
    Task UpdateAsync(Credential entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
