using GaniPay.Payments.Application.Abstractions;
using GaniPay.Payments.Application.Abstractions.Repositories;
using GaniPay.Payments.Application.Services;
using GaniPay.Payments.Infrastructure.Persistence;
using GaniPay.Payments.Infrastructure.Repositories;
using GaniPay.Payments.Infrastructure.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Payments.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("PaymentsDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:PaymentsDb is missing.");

        services.AddDbContext<PaymentsDbContext>(opt => opt.UseNpgsql(cs));

        // Repos + UoW
        services.AddScoped<IPaymentProcessRepository, PaymentProcessRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Workflow client (MVP Noop)
        services.AddScoped<IWorkflowClient, NoopWorkflowClient>();

        // Application service
        services.AddScoped<IPaymentsService, PaymentsService>();

        return services;
    }
}
