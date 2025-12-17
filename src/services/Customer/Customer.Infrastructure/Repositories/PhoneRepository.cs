using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Domain.Entities;
using GaniPay.Customer.Infrastructure.Persistence;

namespace GaniPay.Customer.Infrastructure.Repositories;

public sealed class PhoneRepository : IPhoneRepository
{
    private readonly CustomerDbContext _db;
    public PhoneRepository(CustomerDbContext db) => _db = db;

    public async Task AddAsync(Phone entity, CancellationToken ct) =>
        await _db.Phones.AddAsync(entity, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
