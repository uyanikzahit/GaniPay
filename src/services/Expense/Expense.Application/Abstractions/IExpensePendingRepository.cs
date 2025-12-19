using GaniPay.Expense.Domain.Entities;

namespace GaniPay.Expense.Application.Abstractions;

public interface IExpensePendingRepository
{
    Task<List<ExpensePending>> ListAsync(CancellationToken ct);
    Task<ExpensePending?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(ExpensePending entity, CancellationToken ct);
    Task UpdateAsync(ExpensePending entity, CancellationToken ct);
}
