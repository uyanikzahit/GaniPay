using GaniPay.TransactionLimit.Domain.Entities;

namespace GaniPay.TransactionLimit.Application.Abstractions;

public interface ICustomerLimitRepository
{
    Task<CustomerLimit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerLimit>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    // CHECK için: müşteri + limit tanımı + dönem kırılımı
    Task<CustomerLimit?> GetByCustomerAndDefinitionAsync(
        Guid customerId,
        Guid limitDefinitionId,
        short? year,
        short? month,
        short? day,
        CancellationToken ct = default);

    Task AddAsync(CustomerLimit entity, CancellationToken ct = default);
}
