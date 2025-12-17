using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using CustomerEntity = GaniPay.Customer.Domain.Entities.Customer;

namespace GaniPay.Customer.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _db;

    public CustomerRepository(CustomerDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(CustomerEntity customer, CancellationToken ct)
    {
        await _db.Customers.AddAsync(customer, ct);
    }

    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Customers
            .Include(x => x.Individual)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<CustomerEntity?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(x => x.CustomerNumber == customerNumber, ct);
    }

    public async Task<bool> ExistsByIdentityNumberAsync(string identityNumber, CancellationToken ct)
    {
        return await _db.CustomerIndividuals
            .AnyAsync(x => x.IdentityNumber == identityNumber, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
