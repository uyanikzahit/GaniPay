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
    public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("AccountingDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:AccountingDb is missing.");

        services.AddDbContext<AccountingDbContext>(opt => opt.UseNpgsql(cs));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
        services.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountingService, AccountingService>();

        return services;
    }
}
