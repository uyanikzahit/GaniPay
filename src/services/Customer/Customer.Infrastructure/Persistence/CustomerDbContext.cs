using GaniPay.Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using CustomerEntity = GaniPay.Customer.Domain.Entities.Customer;

namespace GaniPay.Customer.Infrastructure.Persistence;

public sealed class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<CustomerIndividual> CustomerIndividuals => Set<CustomerIndividual>();
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
