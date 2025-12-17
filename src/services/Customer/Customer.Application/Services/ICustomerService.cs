using GaniPay.Customer.Application.Contracts.Dtos;
using GaniPay.Customer.Application.Contracts.Requests;

namespace GaniPay.Customer.Application.Services;

public interface ICustomerService
{
    Task<CustomerDto> CreateIndividualAsync(CreateIndividualCustomerRequest request, CancellationToken ct);
    Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct);

    Task AddEmailAsync(Guid customerId, AddEmailRequest request, CancellationToken ct);
    Task AddPhoneAsync(Guid customerId, AddPhoneRequest request, CancellationToken ct);
    Task AddAddressAsync(Guid customerId, AddAddressRequest request, CancellationToken ct);

    Task CloseAsync(Guid customerId, CloseCustomerRequest request, CancellationToken ct);
}
