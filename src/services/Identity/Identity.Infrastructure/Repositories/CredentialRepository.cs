using GaniPay.Identity.Domain.Entities;
using GaniPay.Identity.Infrastructure.Persistence;
using GaniPay.Identity.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Identity.Infrastructure.Repositories;

public sealed class CredentialRepository : ICredentialRepository
{
    private readonly IdentityDbContext _db;

    public CredentialRepository(IdentityDbContext db) => _db = db;

    public Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Credentials.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Credential?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        => _db.Credentials.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);

    public async Task AddAsync(Credential entity, CancellationToken ct = default)
        => await _db.Credentials.AddAsync(entity, ct);

    public Task UpdateAsync(Credential entity, CancellationToken ct = default)
    {
        _db.Credentials.Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
