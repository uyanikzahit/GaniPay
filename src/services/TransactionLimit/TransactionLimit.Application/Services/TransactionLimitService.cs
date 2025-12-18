using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Contracts.Dtos;
using GaniPay.TransactionLimit.Application.Contracts.Requests;
using GaniPay.TransactionLimit.Application.Contracts.Enums;
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

    public async Task<LimitDefinitionDto> CreateLimitDefinitionAsync(CreateLimitDefinitionRequest request, CancellationToken ct)
    {var exists = await _limitDefinitionRepository.GetByCodeAsync(request.Code, ct);
        if (exists is not null)
            throw new InvalidOperationException("Code already exists");

        var entity = new LimitDefinition
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Period = (GaniPay.TransactionLimit.Domain.Enums.LimitPeriod)request.Period,
            MetricType = (GaniPay.TransactionLimit.Domain.Enums.LimitMetricType)request.MetricType,
            IsVisible = request.IsVisible
        };

        await _limitDefinitionRepository.AddAsync(entity, ct);

        return new LimitDefinitionDto(entity.Id, entity.Code, entity.Name, entity.Description, entity.Period.ToString(), entity.MetricType.ToString(), entity.IsVisible);
    }

    public async Task<CustomerLimitDto> CreateCustomerLimitAsync(CreateCustomerLimitRequest request, CancellationToken ct)
    {var entity = new CustomerLimit
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            LimitDefinitionId = request.LimitDefinitionId,
            Year = request.Year,
            Month = request.Month,
            Day = request.Day,
            Value = request.Value,
            Currency = request.Currency,
            Source = (GaniPay.TransactionLimit.Domain.Enums.LimitSource)request.Source,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            UpdatedBy = request.UpdatedBy
        };

        await _customerLimitRepository.AddAsync(entity, ct);

        return new CustomerLimitDto(
            entity.Id, entity.CustomerId, entity.LimitDefinitionId,
            entity.Year, entity.Month, entity.Day,
            entity.Value, entity.Currency, entity.Source.ToString(),
            entity.Reason, entity.CreatedAt, entity.UpdatedAt, entity.UpdatedBy
        );
    }

    public async Task<IReadOnlyList<CustomerLimitDto>> GetCustomerLimitsAsync(Guid customerId, CancellationToken ct)
{
    var entities = await _customerLimitRepository.ListByCustomerIdAsync(customerId, ct);

    return entities
        .Select(entity => new CustomerLimitDto(
            entity.Id, entity.CustomerId, entity.LimitDefinitionId,
            entity.Year, entity.Month, entity.Day,
            entity.Value, entity.Currency, entity.Source.ToString(),
            entity.Reason, entity.CreatedAt, entity.UpdatedAt, entity.UpdatedBy
        ))
        .ToList();
}
    public async Task<LimitCheckResultDto> CheckAsync(LimitCheckRequest request, CancellationToken ct)
    {
        var def = await _limitDefinitionRepository.GetByCodeAsync(request.Code, ct);
        if (def is null)
            return new LimitCheckResultDto(false, "limit definition not found", null, request.Amount);

        var limits = await _customerLimitRepository.ListByCustomerIdAsync(request.CustomerId, ct);
        var match = limits.FirstOrDefault(x => x.LimitDefinitionId == def.Id);

        if (match is null)
            return new LimitCheckResultDto(true, "no customer-specific limit (MVP default allow)", null, request.Amount);

        if (def.MetricType == GaniPay.TransactionLimit.Domain.Enums.LimitMetricType.Amount && request.Amount > match.Value)
            return new LimitCheckResultDto(false, "limit exceeded", match.Value, request.Amount);

        return new LimitCheckResultDto(true, "ok", match.Value, request.Amount);
    }
}





