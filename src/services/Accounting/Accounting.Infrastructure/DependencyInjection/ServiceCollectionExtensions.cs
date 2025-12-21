using GaniPay.Accounting.Application.Abstractions;
using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Application.Services;
using GaniPay.Accounting.Infrastructure.Persistence;
using GaniPay.Accounting.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Accounting.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Connection string'i net þekilde çek
        var connStr =
            configuration.GetConnectionString("AccountingDb")
            ?? configuration.GetConnectionString("ganipay_accounting")
            ?? configuration["ConnectionStrings:AccountingDb"]
            ?? configuration["ConnectionStrings:ganipay_accounting"];

        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException(
                "Accounting DB connection string bulunamadý. " +
                "appsettings.json içine ConnectionStrings:AccountingDb ekle.");

        // DbContext REGISTER (en kritik kýsým)
        services.AddDbContext<AccountingDbContext>(opt =>
        {
            opt.UseNpgsql(connStr, npgsql =>
            {
                // Migration'lar Infrastructure assembly içinde
                npgsql.MigrationsAssembly(typeof(AccountingDbContext).Assembly.FullName);
            });

            // Ýstersen debug için aç:
            // opt.EnableSensitiveDataLogging();
            // opt.EnableDetailedErrors();
        });

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
        services.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        services.AddScoped<IAccountingService, AccountingService>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
