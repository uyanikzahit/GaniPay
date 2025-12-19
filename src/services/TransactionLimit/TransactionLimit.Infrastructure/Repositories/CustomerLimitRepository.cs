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
        => _db.CustomerLimits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CustomerLimit>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await _db.CustomerLimits
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<CustomerLimit?> GetByCustomerAndDefinitionAsync(
        Guid customerId,
        Guid limitDefinitionId,
        short? year,
        short? month,
        short? day,
        CancellationToken ct = default)
        => _db.CustomerLimits
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customerId &&
                x.LimitDefinitionId == limitDefinitionId &&
                x.Year == year &&
                x.Month == month &&
                x.Day == day, ct);

    public async Task AddAsync(CustomerLimit entity, CancellationToken ct = default)
    {
        _db.CustomerLimits.Add(entity);
        await _db.SaveChangesAsync(ct);
    }
}
