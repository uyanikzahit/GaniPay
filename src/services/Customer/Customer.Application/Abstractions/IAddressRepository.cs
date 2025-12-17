using GaniPay.Customer.Domain.Entities;

namespace GaniPay.Customer.Application.Abstractions;

public interface IAddressRepository
{
    Task AddAsync(Address entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
