using GaniPay.Payments.Application.Abstractions.Repositories;
using GaniPay.Payments.Domain.Entities;
using GaniPay.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Payments.Infrastructure.Repositories;

public sealed class PaymentProcessRepository : IPaymentProcessRepository
{
    private readonly PaymentsDbContext _db;

    public PaymentProcessRepository(PaymentsDbContext db)
    {
        _db = db;
    }

    public Task<PaymentProcess?> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
        => _db.PaymentProcesses.FirstOrDefaultAsync(x => x.CorrelationId == correlationId, ct);

    public Task<PaymentProcess?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => _db.PaymentProcesses.FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);

    public async Task AddAsync(PaymentProcess process, CancellationToken ct = default)
        => await _db.PaymentProcesses.AddAsync(process, ct);

    public Task UpdateAsync(PaymentProcess process, CancellationToken ct = default)
    {
        _db.PaymentProcesses.Update(process);
        return Task.CompletedTask;
    }
}
