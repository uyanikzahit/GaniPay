using GaniPay.Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GaniPay.Expense.Infrastructure.Persistence;

public sealed class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

    public DbSet<ExpenseDefinition> Expenses => Set<ExpenseDefinition>();
    public DbSet<ExpensePending> ExpensePendings => Set<ExpensePending>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseDbContext).Assembly);
    }
}
