using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Domain.Entities;
using GaniPay.Customer.Infrastructure.Persistence;

namespace GaniPay.Customer.Infrastructure.Repositories;

public sealed class EmailRepository : IEmailRepository
{
    private readonly CustomerDbContext _db;
    public EmailRepository(CustomerDbContext db) => _db = db;

    public async Task AddAsync(Email entity, CancellationToken ct) =>
        await _db.Emails.AddAsync(entity, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
