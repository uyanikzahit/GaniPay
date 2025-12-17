using GaniPay.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<CredentialRecovery> CredentialRecoveries => Set<CredentialRecovery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
