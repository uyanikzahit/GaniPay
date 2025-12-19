using GaniPay.Expense.Application.Contracts.Dtos;
using GaniPay.Expense.Application.Requests;

namespace GaniPay.Expense.Application.Services;

public interface IExpenseService
{
    Task<List<ExpenseDto>> ListAsync(CancellationToken ct);
    Task<ExpenseDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct);
    Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<decimal> CalculateAsync(CalculateExpenseRequest request, CancellationToken ct);
}
