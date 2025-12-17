using GaniPay.Customer.Domain.Entities;

namespace GaniPay.Customer.Application.Abstractions;

public interface IPhoneRepository
{
    Task AddAsync(Phone entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
