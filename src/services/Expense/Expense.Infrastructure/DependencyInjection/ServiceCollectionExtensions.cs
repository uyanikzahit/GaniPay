using GaniPay.Expense.Application.Abstractions;
using GaniPay.Expense.Application.Services;
using GaniPay.Expense.Infrastructure.Persistence;
using GaniPay.Expense.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaniPay.Expense.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExpenseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("ExpenseDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:ExpenseDb bulunamadý.");

        services.AddDbContext<ExpenseDbContext>(opt => opt.UseNpgsql(cs));

        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpensePendingRepository, ExpensePendingRepository>();

        services.AddScoped<IExpenseService, ExpenseService>();

        return services;
    }
}
