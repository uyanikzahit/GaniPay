using GaniPay.Accounting.Domain.Entities;

namespace GaniPay.Accounting.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<bool> ExistsAsync(Guid customerId, string currency, CancellationToken ct);

    Task<Account?> GetByCustomerAndCurrencyAsync(Guid customerId, string currency, CancellationToken ct);

    Task AddAsync(Account account, CancellationToken ct);

    Task UpdateAsync(Account account, CancellationToken ct);
}
