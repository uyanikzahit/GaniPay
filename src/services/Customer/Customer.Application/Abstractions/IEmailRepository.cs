using GaniPay.Customer.Domain.Entities;

namespace GaniPay.Customer.Application.Abstractions;

public interface IEmailRepository
{
    Task AddAsync(Email entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
