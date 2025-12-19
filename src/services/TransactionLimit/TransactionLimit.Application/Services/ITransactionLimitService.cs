using GaniPay.TransactionLimit.Application.Contracts.Dtos;
using GaniPay.TransactionLimit.Application.Contracts.Requests;

namespace GaniPay.TransactionLimit.Application.Services;

public interface ITransactionLimitService
{
    Task<IReadOnlyList<LimitDefinitionDto>> GetLimitDefinitionsAsync(CancellationToken ct);
    Task<LimitDefinitionDto> CreateLimitDefinitionAsync(CreateLimitDefinitionRequest request, CancellationToken ct);

    Task<IReadOnlyList<CustomerLimitDto>> GetCustomerLimitsAsync(Guid customerId, CancellationToken ct);
    Task<CustomerLimitDto> CreateCustomerLimitAsync(CreateCustomerLimitRequest request, CancellationToken ct);

    Task<LimitCheckResultDto> CheckAsync(LimitCheckRequest request, CancellationToken ct);
}
