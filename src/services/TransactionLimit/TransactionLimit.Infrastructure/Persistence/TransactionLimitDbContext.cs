using GaniPay.TransactionLimit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.TransactionLimit.Infrastructure.Persistence;

public sealed class TransactionLimitDbContext : DbContext
{
    public TransactionLimitDbContext(DbContextOptions<TransactionLimitDbContext> options) : base(options) { }

    public DbSet<LimitDefinition> LimitDefinitions => Set<LimitDefinition>();
    public DbSet<CustomerLimit> CustomerLimits => Set<CustomerLimit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionLimitDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
