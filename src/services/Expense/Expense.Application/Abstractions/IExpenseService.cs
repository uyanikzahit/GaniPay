using GaniPay.Expense.Application.Contracts.Dtos;
using GaniPay.Expense.Application.Requests;

namespace GaniPay.Expense.Application.Abstractions;

public interface IExpenseService
{
    // ExpenseDefinition
    Task<List<ExpenseDto>> ListAsync(CancellationToken ct);
    Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ExpenseDto> GetByCodeAsync(string code, CancellationToken ct);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct);
    Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);

    // Pending (fee calculation result)
    Task<ExpensePendingDto> CreatePendingAsync(CreateExpensePendingRequest request, CancellationToken ct);
}
