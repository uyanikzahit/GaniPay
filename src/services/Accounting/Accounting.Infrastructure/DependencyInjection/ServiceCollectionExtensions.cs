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
    public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("AccountingDb")
                 ?? throw new InvalidOperationException("ConnectionStrings:AccountingDb is missing.");

        services.AddDbContext<AccountingDbContext>(opt =>
        {
            opt.UseNpgsql(cs);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
        services.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();

        services.AddScoped<IAccountingService, AccountingService>();

        return services;
    }
}
