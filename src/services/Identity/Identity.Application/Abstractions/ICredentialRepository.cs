using GaniPay.Identity.Domain.Entities;

namespace GaniPay.Identity.Application.Abstractions;

public interface ICredentialRepository
{
    Task<bool> ExistsByLoginAsync(string loginType, string loginValue, CancellationToken ct = default);
    Task<Credential?> GetByLoginAsync(string loginType, string loginValue, CancellationToken ct = default);
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Credential entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
