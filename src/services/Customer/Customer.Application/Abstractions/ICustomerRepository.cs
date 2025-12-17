using CustomerEntity = GaniPay.Customer.Domain.Entities.Customer;

namespace GaniPay.Customer.Application.Abstractions;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<CustomerEntity?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct);

    Task<bool> ExistsByIdentityNumberAsync(string identityNumber, CancellationToken ct);

    Task AddAsync(CustomerEntity customer, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
