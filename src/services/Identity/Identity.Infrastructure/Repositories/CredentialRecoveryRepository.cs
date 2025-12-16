using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Infrastructure.Persistence;
using GaniPay.Identity.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Identity.Infrastructure.Repositories;

public sealed class CredentialRecoveryRepository : ICredentialRecoveryRepository
{
    private readonly IdentityDbContext _db;

    public CredentialRecoveryRepository(IdentityDbContext db) => _db = db;

    public Task<CredentialRecovery?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.CredentialRecoveries.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(CredentialRecovery entity, CancellationToken ct = default)
        => await _db.CredentialRecoveries.AddAsync(entity, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
