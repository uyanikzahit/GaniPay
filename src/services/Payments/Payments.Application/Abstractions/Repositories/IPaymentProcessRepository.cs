using GaniPay.Payments.Domain.Entities;

namespace GaniPay.Payments.Application.Abstractions.Repositories;

public interface IPaymentProcessRepository
{
    Task<PaymentProcess?> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default);
    Task<PaymentProcess?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    Task AddAsync(PaymentProcess process, CancellationToken ct = default);
    Task UpdateAsync(PaymentProcess process, CancellationToken ct = default);
}
