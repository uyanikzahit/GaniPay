using GaniPay.TransactionLimit.Domain.Entities;

namespace GaniPay.TransactionLimit.Application.Abstractions;

public interface ICustomerLimitRepository
{
    Task<CustomerLimit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerLimit>> ListByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(CustomerLimit entity, CancellationToken ct = default);
    Task UpdateAsync(CustomerLimit entity, CancellationToken ct = default);
    Task DeleteAsync(CustomerLimit entity, CancellationToken ct = default);
}
