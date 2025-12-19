using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Services;
using GaniPay.TransactionLimit.Infrastructure.Persistence;
using GaniPay.TransactionLimit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.TransactionLimit.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionLimitInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("TransactionLimitDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:TransactionLimitDb not found.");

        services.AddDbContext<TransactionLimitDbContext>(opt =>
        {
            opt.UseNpgsql(cs);
        });

        services.AddScoped<ILimitDefinitionRepository, LimitDefinitionRepository>();
        services.AddScoped<ICustomerLimitRepository, CustomerLimitRepository>();

        services.AddScoped<ITransactionLimitService, TransactionLimitService>();

        return services;
    }
}
