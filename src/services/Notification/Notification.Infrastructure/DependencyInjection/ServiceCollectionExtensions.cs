using GaniPay.Notification.Application.Abstractions;
using GaniPay.Notification.Application.Abstractions.Providers;
using GaniPay.Notification.Application.Abstractions.Repositories;
using GaniPay.Notification.Application.Services;
using GaniPay.Notification.Infrastructure.Persistence;
using GaniPay.Notification.Infrastructure.Providers;
using GaniPay.Notification.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Notification.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("NotificationDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:NotificationDb is missing.");

        services.AddDbContext<NotificationDbContext>(opt => opt.UseNpgsql(cs));

        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<INotificationProvider, MockNotificationProvider>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
