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
        => _db.LimitDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<LimitDefinition?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.LimitDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<LimitDefinition>> GetAllAsync(CancellationToken ct = default)
        => await _db.LimitDefinitions.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    public async Task AddAsync(LimitDefinition entity, CancellationToken ct = default)
    {
        _db.LimitDefinitions.Add(entity);
        await _db.SaveChangesAsync(ct);
    }
}
