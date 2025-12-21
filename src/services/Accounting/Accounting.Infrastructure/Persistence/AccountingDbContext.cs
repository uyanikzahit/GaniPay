using Microsoft.EntityFrameworkCore;
using DomainAccount = GaniPay.Accounting.Domain.Entities.Account;
using DomainTx = GaniPay.Accounting.Domain.Entities.AccountingTransaction;
using DomainHistory = GaniPay.Accounting.Domain.Entities.AccountBalanceHistory;

namespace GaniPay.Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }

    public DbSet<DomainAccount> Accounts => Set<DomainAccount>();
    public DbSet<DomainTx> AccountingTransactions => Set<DomainTx>();
    public DbSet<DomainHistory> AccountBalanceHistories => Set<DomainHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
