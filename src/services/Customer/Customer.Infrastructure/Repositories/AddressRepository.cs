using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Domain.Entities;
using GaniPay.Customer.Infrastructure.Persistence;

namespace GaniPay.Customer.Infrastructure.Repositories;

public sealed class AddressRepository : IAddressRepository
{
    private readonly CustomerDbContext _db;

    public AddressRepository(CustomerDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Address entity, CancellationToken ct)
    {
        await _db.Addresses.AddAsync(entity, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
