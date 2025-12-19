using GaniPay.TransactionLimit.Application.Abstractions;
using GaniPay.TransactionLimit.Application.Contracts.Dtos;
using GaniPay.TransactionLimit.Application.Contracts.Enums;
using GaniPay.TransactionLimit.Application.Contracts.Requests;

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
            .Select(x => new LimitDefinitionDto(
                x.Id,
                x.Code,
                x.Name,
                x.Description ?? string.Empty,
                MapToContract(x.Period),
                MapToContract(x.MetricType),
                x.IsVisible))
            .ToList();
    }

    public async Task<LimitDefinitionDto> CreateLimitDefinitionAsync(CreateLimitDefinitionRequest request, CancellationToken ct)
    {
        var exists = await _limitDefinitionRepository.GetByCodeAsync(request.Code, ct);
        if (exists is not null)
            throw new InvalidOperationException("Code already exists");

        var entity = new Domain.Entities.LimitDefinition
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Period = MapToDomain(request.Period),
            MetricType = MapToDomain(request.MetricType),
            IsVisible = request.IsVisible
        };

        await _limitDefinitionRepository.AddAsync(entity, ct);

        return new LimitDefinitionDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description ?? string.Empty,
            request.Period,
            request.MetricType,
            entity.IsVisible);
    }

    public async Task<IReadOnlyList<CustomerLimitDto>> GetCustomerLimitsAsync(Guid customerId, CancellationToken ct)
    {
        var items = await _customerLimitRepository.GetByCustomerIdAsync(customerId, ct);

        return items
            .Select(x => new CustomerLimitDto(
                x.Id,
                x.CustomerId,
                x.LimitDefinitionId,
                x.Year.GetValueOrDefault(),
                x.Month,
                x.Day,
                x.Value,
                x.Currency ?? "TRY",
                MapToContract(x.Source),
                x.Reason ?? string.Empty,          // ✅ CS8604 fix
                x.CreatedAt,
                x.UpdatedAt,
                x.UpdatedBy))
            .ToList();
    }

    public async Task<CustomerLimitDto> CreateCustomerLimitAsync(Guid customerId, CreateCustomerLimitRequest request, CancellationToken ct)
    {
        if (request.CustomerId != Guid.Empty && request.CustomerId != customerId)
            throw new InvalidOperationException("customerId path/body mismatch");

        var def = await _limitDefinitionRepository.GetByIdAsync(request.LimitDefinitionId, ct);
        if (def is null)
            throw new InvalidOperationException("LimitDefinition not found");

        var (year, month, day) = NormalizePeriod(MapToContract(def.Period), request.Year, request.Month, request.Day);

        var entity = new Domain.Entities.CustomerLimit
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            LimitDefinitionId = request.LimitDefinitionId,
            Year = year,
            Month = month,
            Day = day,
            Value = request.Value,
            Currency = request.Currency ?? "TRY",
            Source = MapToDomain(request.Source),
            Reason = request.Reason ?? string.Empty,   // ✅ null-safe
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            UpdatedBy = request.UpdatedBy
        };

        await _customerLimitRepository.AddAsync(entity, ct);

        return new CustomerLimitDto(
            entity.Id,
            entity.CustomerId,
            entity.LimitDefinitionId,
            entity.Year.GetValueOrDefault(),
            entity.Month,
            entity.Day,
            entity.Value,
            entity.Currency ?? "TRY",
            MapToContract(entity.Source),
            entity.Reason ?? string.Empty,             // ✅ null-safe
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.UpdatedBy);
    }

    public async Task<LimitCheckResultDto> CheckAsync(LimitCheckRequest request, CancellationToken ct)
    {
        var def = await _limitDefinitionRepository.GetByIdAsync(request.LimitDefinitionId, ct);
        if (def is null)
            return new LimitCheckResultDto(false, "LimitDefinition not found", request.Value, null);

        var period = MapToContract(def.Period);
        var (year, month, day) = NormalizePeriod(period, request.Year, request.Month, request.Day);

        var limits = await _customerLimitRepository.GetByCustomerIdAsync(request.CustomerId, ct);

        var matched = limits.FirstOrDefault(x =>
            x.LimitDefinitionId == request.LimitDefinitionId &&
            x.Year.GetValueOrDefault() == year &&
            x.Month == month &&
            x.Day == day);

        if (matched is null)
            return new LimitCheckResultDto(true, "No customer-specific limit found; allowed", request.Value, null);

        var allowed = request.Value <= matched.Value;

        return new LimitCheckResultDto(
            allowed,
            allowed ? "Allowed" : "Limit exceeded",
            request.Value,
            matched.Value);
    }

    private static (short year, short? month, short? day) NormalizePeriod(LimitPeriod period, short year, short? month, short? day)
        => period switch
        {
            LimitPeriod.Year => (year, null, null),
            LimitPeriod.Month => (year, month, null),
            _ => (year, month, day)
        };

    private static Domain.Enums.LimitPeriod MapToDomain(LimitPeriod p) => p switch
    {
        LimitPeriod.Year => Domain.Enums.LimitPeriod.Year,
        LimitPeriod.Month => Domain.Enums.LimitPeriod.Month,
        _ => Domain.Enums.LimitPeriod.Day
    };

    private static Domain.Enums.LimitMetricType MapToDomain(LimitMetricType m) => m switch
    {
        LimitMetricType.Count => Domain.Enums.LimitMetricType.Count,
        LimitMetricType.Balance => Domain.Enums.LimitMetricType.Balance,
        _ => Domain.Enums.LimitMetricType.Amount
    };

    private static Domain.Enums.LimitSource MapToDomain(LimitSource s) => s switch
    {
        LimitSource.System => Domain.Enums.LimitSource.System,
        LimitSource.Migration => Domain.Enums.LimitSource.Migration,
        _ => Domain.Enums.LimitSource.Admin
    };

    private static LimitPeriod MapToContract(Domain.Enums.LimitPeriod p) => p switch
    {
        Domain.Enums.LimitPeriod.Year => LimitPeriod.Year,
        Domain.Enums.LimitPeriod.Month => LimitPeriod.Month,
        _ => LimitPeriod.Day
    };

    private static LimitMetricType MapToContract(Domain.Enums.LimitMetricType m) => m switch
    {
        Domain.Enums.LimitMetricType.Count => LimitMetricType.Count,
        Domain.Enums.LimitMetricType.Balance => LimitMetricType.Balance,
        _ => LimitMetricType.Amount
    };

    private static LimitSource MapToContract(Domain.Enums.LimitSource s) => s switch
    {
        Domain.Enums.LimitSource.System => LimitSource.System,
        Domain.Enums.LimitSource.Migration => LimitSource.Migration,
        _ => LimitSource.Admin
    };
}
