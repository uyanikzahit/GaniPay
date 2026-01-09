using GaniPay.Accounting.Domain.Entities;

namespace GaniPay.Accounting.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default);
    Task<Account?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct = default);

    Task AddAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);

    Task<List<Account>> ListByCustomerIdAsync(Guid customerId, CancellationToken ct);

}
