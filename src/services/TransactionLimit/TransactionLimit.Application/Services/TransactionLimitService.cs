using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Contracts.Dtos;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Domain.Entities;

namespace GaniPay.TransactionLimit.Application.Services;

public sealed class TransactionLimitService : ITransactionLimitService
{
    private readonly ILimitDefinitionRepository _limitDefinitionRepository;
    private readonly ICustomerLimitRepository _customerLimitRepository;

    public TransactionLimitService(
        ILimitDefinitionRepository limitDefinitionRepository,
        ICustomerLimitRepository customerLimitRepository)
    {
        _limitDefinitionRepository = limitDefinitionRepository;
        _customerLimitRepository = customerLimitRepository;
    }

    public async Task<IReadOnlyList<LimitDefinitionDto>> GetLimitDefinitionsAsync(CancellationToken ct)
    {
        var items = await _limitDefinitionRepository.GetAllAsync(ct);
        return items
            .Select(x => new LimitDefinitionDto(x.Id, x.Code, x.Name, x.Description, x.Period.ToString(), x.MetricType.ToString(), x.IsVisible))
            .ToList();
    }

    public async Task<IReadOnlyList<CustomerLimitDto>> GetCustomerLimitsAsync(Guid customerId, CancellationToken ct)
    {
        var items = await _customerLimitRepository.GetByCustomerIdAsync(customerId, ct);
        return items
            .Select(x => new CustomerLimitDto(
                x.Id, x.CustomerId, x.LimitDefinitionId, x.Year, x.Month, x.Day,
                x.Value, x.Currency, x.Source.ToString(), x.Reason, x.CreatedAt, x.UpdatedAt, x.UpdatedBy))
            .ToList();
    }

    public async Task<LimitCheckResultDto> CheckAsync(LimitCheckRequest request, CancellationToken ct)
    {
        // Tanımı bul
        var def = await _limitDefinitionRepository.GetByIdAsync(request.LimitDefinitionId, ct);
        if (def is null)
            return new LimitCheckResultDto(false, "LimitDefinition bulunamadı.", request.Value, null);

        // Dönem kırılımını definition.period'e göre normalize et
        short? year = request.Year;
        short? month = request.Month;
        short? day = request.Day;

        var p = def.Period.ToString().ToLowerInvariant();
        if (p == "year")
        {
            month = null; day = null;
        }
        else if (p == "month")
        {
            day = null;
        }
        else if (p == "day")
        {
            // year+month+day beklenir, sende null geliyorsa swagger’dan doldur
        }

        // Müşteri limiti
        var limit = await _customerLimitRepository.GetByCustomerAndDefinitionAsync(
            request.CustomerId, request.LimitDefinitionId, year, month, day, ct);

        if (limit is null)
            return new LimitCheckResultDto(true, "Bu müşteri için limit tanımlı değil. (ALLOW)", request.Value, null);

        // Basit MVP kural: request.value <= limit.value
        var allowed = request.Value <= limit.Value;
        return allowed
            ? new LimitCheckResultDto(true, "Limit uygun. (ALLOW)", request.Value, limit.Value)
            : new LimitCheckResultDto(false, "Limit aşıldı. (DENY)", request.Value, limit.Value);
    }

    // Sende var olan CreateLimitDefinitionAsync / CreateCustomerLimitAsync aynen kalsın
}
