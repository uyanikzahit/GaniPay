using GaniPay.Customer.Domain.Entities;

namespace GaniPay.Customer.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Customer?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct);

    // Individual kayýt tekilliði için (TCKN gibi)
    Task<bool> ExistsByIdentityNumberAsync(string identityNumber, CancellationToken ct);

    Task AddAsync(Customer customer, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
