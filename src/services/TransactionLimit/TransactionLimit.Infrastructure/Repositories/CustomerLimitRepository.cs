using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Domain.Entities;
using GaniPay.TransactionLimit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.TransactionLimit.Infrastructure.Repositories;

public sealed class CustomerLimitRepository : ICustomerLimitRepository
{
    private readonly TransactionLimitDbContext _db;

    public CustomerLimitRepository(TransactionLimitDbContext db) => _db = db;

    public Task<CustomerLimit?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.CustomerLimits
            .Include(x => x.LimitDefinition)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CustomerLimit>> ListByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await _db.CustomerLimits
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(ct);

    public async Task AddAsync(CustomerLimit entity, CancellationToken ct = default)
    {
        _db.CustomerLimits.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CustomerLimit entity, CancellationToken ct = default)
    {
        _db.CustomerLimits.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(CustomerLimit entity, CancellationToken ct = default)
    {
        _db.CustomerLimits.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
