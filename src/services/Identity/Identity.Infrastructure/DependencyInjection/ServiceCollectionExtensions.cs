using GaniPay.Identity.Application.Abstractions;
using GaniPay.Identity.Application.Security;
using GaniPay.Identity.Application.Services;
using GaniPay.Identity.Infrastructure.Persistence;
using GaniPay.Identity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Identity.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("IdentityDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:IdentityDb tanýmlý deðil.");

        services.AddDbContext<IdentityDbContext>(opt => opt.UseNpgsql(cs));

        // Repos
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<ICredentialRecoveryRepository, CredentialRecoveryRepository>();

        // App services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IdentityService>();

        return services;
    }
}
