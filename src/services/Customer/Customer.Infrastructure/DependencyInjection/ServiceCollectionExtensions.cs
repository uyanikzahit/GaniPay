using GaniPay.Customer.Application.Abstractions;
using GaniPay.Customer.Infrastructure.Persistence;
using GaniPay.Customer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Customer.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CustomerDb")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IPhoneRepository, PhoneRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();

        return services;
    }
}
