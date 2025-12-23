using GaniPay.Integration.Application.Abstractions;
using GaniPay.Integration.Application.Abstractions.Providers;
using GaniPay.Integration.Application.Abstractions.Repositories;
using GaniPay.Integration.Application.Services;
using GaniPay.Integration.Infrastructure.Persistence;
using GaniPay.Integration.Infrastructure.Providers;
using GaniPay.Integration.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Integration.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("IntegrationDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:IntegrationDb is missing.");

        services.AddDbContext<IntegrationDbContext>(opt => opt.UseNpgsql(cs));

        services.AddScoped<IIntegrationProviderRepository, IntegrationProviderRepository>();
        services.AddScoped<IIntegrationLogRepository, IntegrationLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IIntegrationProviderClient, MockBankProvider>();

        // Application service
        services.AddScoped<IIntegrationService, IntegrationService>();

        return services;
    }
}
