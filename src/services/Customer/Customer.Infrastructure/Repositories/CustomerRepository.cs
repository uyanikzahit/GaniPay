using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Domain.Entities;
using GaniPay.Customer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Customer.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _db;
    public CustomerRepository(CustomerDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Customers
           .Include(x => x.Individual)
           .Include(x => x.Emails)
           .Include(x => x.Phones)
           .Include(x => x.Addresses)
           .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Customer?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(x => x.CustomerNumber == customerNumber, ct);

    public Task<bool> ExistsByIdentityNumberAsync(string identityNumber, CancellationToken ct) =>
        _db.CustomerIndividuals.AnyAsync(x => x.IdentityNumber == identityNumber, ct);

    public async Task AddAsync(Customer customer, CancellationToken ct) =>
        await _db.Customers.AddAsync(customer, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
