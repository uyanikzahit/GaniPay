using GaniPay.Identity.Application.Abstractions;
using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Identity.Infrastructure.Repositories;

public sealed class CredentialRecoveryRepository : ICredentialRecoveryRepository
{
    private readonly IdentityDbContext _db;

    public CredentialRecoveryRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(CredentialRecovery entity, CancellationToken ct = default)
    {
        await _db.CredentialRecoveries.AddAsync(entity, ct);
    }

    // KRÝTÝK: FindAsync deðil! TokenHash alanýndan arýyoruz.
    public Task<CredentialRecovery?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return _db.CredentialRecoveries
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
