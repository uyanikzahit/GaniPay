using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Domain.Entities;
using GaniPay.TransactionLimit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.TransactionLimit.Infrastructure.Repositories;

public sealed class LimitDefinitionRepository : ILimitDefinitionRepository
{
    private readonly TransactionLimitDbContext _db;

    public LimitDefinitionRepository(TransactionLimitDbContext db) => _db = db;

    public Task<LimitDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.LimitDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<LimitDefinition?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.LimitDefinitions.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<LimitDefinition>> ListAsync(CancellationToken ct = default)
        => await _db.LimitDefinitions.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(LimitDefinition entity, CancellationToken ct = default)
    {
        _db.LimitDefinitions.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(LimitDefinition entity, CancellationToken ct = default)
    {
        _db.LimitDefinitions.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(LimitDefinition entity, CancellationToken ct = default)
    {
        _db.LimitDefinitions.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
