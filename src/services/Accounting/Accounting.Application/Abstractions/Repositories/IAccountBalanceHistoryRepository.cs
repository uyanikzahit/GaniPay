using GaniPay.Accounting.Domain.Entities;

namespace GaniPay.Accounting.Application.Abstractions.Repositories;

public interface IAccountBalanceHistoryRepository
{
    Task AddAsync(AccountBalanceHistory history, CancellationToken ct = default);

    Task<IReadOnlyList<AccountBalanceHistory>> ListByAccountAndRangeAsync(
        Guid accountId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);

    Task<IReadOnlyList<AccountBalanceHistory>> ListByAccountIdAsync(
    Guid accountId,
    CancellationToken ct = default);
}
