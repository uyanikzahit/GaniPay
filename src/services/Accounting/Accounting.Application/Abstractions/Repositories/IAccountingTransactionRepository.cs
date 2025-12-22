using GaniPay.Accounting.Domain.Entities;

namespace GaniPay.Accounting.Application.Abstractions.Repositories;

public interface IAccountingTransactionRepository
{
    Task AddAsync(AccountingTransaction tx, CancellationToken ct = default);

    Task<IReadOnlyList<AccountingTransaction>> ListByAccountAndRangeAsync(
        Guid accountId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);

    Task<IReadOnlyList<AccountingTransaction>> ListByCustomerAndCurrencyAndRangeAsync(
        Guid customerId,
        string currency,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);
}
