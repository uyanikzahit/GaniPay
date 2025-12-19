using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Domain.Entities;
using GaniPay.Expense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Expense.Infrastructure.Repositories;

public sealed class ExpensePendingRepository : IExpensePendingRepository
{
    private readonly ExpenseDbContext _db;

    public ExpensePendingRepository(ExpenseDbContext db) => _db = db;

    public Task<ExpensePending?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.ExpensePendings.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(ExpensePending entity, CancellationToken ct)
    {
        _db.ExpensePendings.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ExpensePending entity, CancellationToken ct)
    {
        _db.ExpensePendings.Update(entity);
        await _db.SaveChangesAsync(ct);
    }
}
