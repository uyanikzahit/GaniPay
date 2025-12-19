using GaniPay.Expense.Domain.Entities;

namespace GaniPay.Expense.Application.Abstractions;

public interface IExpenseRepository
{
    Task<List<ExpenseDefinition>> ListAsync(CancellationToken ct);
    Task<ExpenseDefinition?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ExpenseDefinition?> GetByCodeAsync(string code, CancellationToken ct);

    Task AddAsync(ExpenseDefinition entity, CancellationToken ct);
    Task UpdateAsync(ExpenseDefinition entity, CancellationToken ct);
    Task DeleteAsync(ExpenseDefinition entity, CancellationToken ct);
}
