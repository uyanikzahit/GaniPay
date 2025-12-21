using GaniPay.Accounting.Application.Abstractions;
using GaniPay.Accounting.Application.Abstractions.Repositories;
using GaniPay.Accounting.Infrastructure.Persistence;
using GaniPay.Accounting.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Accounting.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("AccountingDb")
                     ?? throw new InvalidOperationException("ConnectionStrings:AccountingDb is missing.");



        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
        services.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
