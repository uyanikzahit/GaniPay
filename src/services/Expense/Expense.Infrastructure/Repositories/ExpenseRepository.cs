using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Domain.Entities;
using GaniPay.Expense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Expense.Infrastructure.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext _db;

    public ExpenseRepository(ExpenseDbContext db) => _db = db;

    public Task<List<ExpenseDefinition>> ListAsync(CancellationToken ct)
        => _db.Expenses.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    public Task<ExpenseDefinition?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ExpenseDefinition?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.Expenses.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task AddAsync(ExpenseDefinition entity, CancellationToken ct)
    {
        _db.Expenses.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ExpenseDefinition entity, CancellationToken ct)
    {
        _db.Expenses.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ExpenseDefinition entity, CancellationToken ct)
    {
        _db.Expenses.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
