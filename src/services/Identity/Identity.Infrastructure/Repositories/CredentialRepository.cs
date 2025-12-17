using GaniPay.Identity.Application.Abstractions;
using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Identity.Infrastructure.Repositories;

public sealed class CredentialRepository : ICredentialRepository
{
    private readonly IdentityDbContext _db;

    public CredentialRepository(IdentityDbContext db) => _db = db;

    public Task<bool> ExistsByLoginAsync(string loginType, string loginValue, CancellationToken ct = default)
        => _db.Credentials.AnyAsync(x => x.LoginType == loginType && x.LoginValue == loginValue, ct);

    public Task<Credential?> GetByLoginAsync(string loginType, string loginValue, CancellationToken ct = default)
        => _db.Credentials.FirstOrDefaultAsync(x => x.LoginType == loginType && x.LoginValue == loginValue, ct);

    public Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Credentials.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Credential entity, CancellationToken ct = default)
        => _db.Credentials.AddAsync(entity, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
