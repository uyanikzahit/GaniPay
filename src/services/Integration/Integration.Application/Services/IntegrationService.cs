using GaniPay.Integration.Application.Abstractions;
using GaniPay.Integration.Application.Abstractions.Providers;
using GaniPay.Integration.Application.Abstractions.Repositories;
using GaniPay.Integration.Application.Contracts.Dtos;
using GaniPay.Integration.Application.Contracts.Requests;
using GaniPay.Integration.Domain.Entities;
using GaniPay.Integration.Domain.Enums;

namespace GaniPay.Integration.Application.Services;

public sealed class IntegrationService : IIntegrationService
{
    private readonly IIntegrationProviderRepository _providerRepo;
    private readonly IIntegrationLogRepository _logRepo;
    private readonly IIntegrationProviderClient _client;
    private readonly IUnitOfWork _uow;

    public IntegrationService(
        IIntegrationProviderRepository providerRepo,
        IIntegrationLogRepository logRepo,
        IIntegrationProviderClient client,
        IUnitOfWork uow)
    {
        _providerRepo = providerRepo;
        _logRepo = logRepo;
        _client = client;
        _uow = uow;
    }

    public async Task<CallIntegrationResultDto> CallAsync(CallIntegrationRequest request, CancellationToken ct)
    {
        ValidateCallRequest(request);

        var providerCode = request.ProviderCode.Trim();
        var operation = request.Operation.Trim();

        // 1) provider resolve
        var provider = await _providerRepo.GetByCodeAsync(providerCode, ct)
                      ?? throw new InvalidOperationException("provider not found");

        if (!provider.IsActive)
            throw new InvalidOperationException("provider is not active");

        // 2) create log (Pending)
        var log = new IntegrationLog
        {
            Id = Guid.NewGuid(),
            ProviderId = provider.Id,
            Operation = operation,
            RequestPayload = request.RequestPayload,
            ResponsePayload = "{}", // placeholder
            Status = IntegrationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _logRepo.AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);

        // 3) call provider (MVP mock)
        try
        {
            var responseJson = await _client.CallAsync(log.Operation, log.RequestPayload, ct);

            log.Status = IntegrationStatus.Success;
            log.ResponsePayload = responseJson;
        }
        catch (Exception ex)
        {
            log.Status = IntegrationStatus.Fail;
            log.ResponsePayload = $"{{\"error\":\"{EscapeJson(ex.Message)}\"}}";
        }

        await _logRepo.UpdateAsync(log, ct);
        await _uow.SaveChangesAsync(ct);

        return new CallIntegrationResultDto(log.Id, log.Status.ToString());
    }

    public async Task<IntegrationLogDto> GetAsync(GetIntegrationLogRequest request, CancellationToken ct)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidOperationException("id is required");

        var log = await _logRepo.GetByIdAsync(request.Id, ct)
                  ?? throw new InvalidOperationException("integration log not found");

        return ToDto(log);
    }

    public async Task<List<IntegrationLogDto>> GetProviderLogsAsync(GetProviderLogsRequest request, CancellationToken ct)
    {
        if (request.ProviderId == Guid.Empty)
            throw new InvalidOperationException("providerId is required");

        var logs = await _logRepo.GetByProviderIdAsync(request.ProviderId, ct);
        return logs.Select(ToDto).ToList();
    }

    private static void ValidateCallRequest(CallIntegrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderCode))
            throw new InvalidOperationException("providerCode is required");

        if (string.IsNullOrWhiteSpace(request.Operation))
            throw new InvalidOperationException("operation is required");

        if (string.IsNullOrWhiteSpace(request.RequestPayload))
            throw new InvalidOperationException("requestPayload is required");
    }

    private static IntegrationLogDto ToDto(IntegrationLog x)
        => new(
            x.Id,
            x.ProviderId,
            x.Operation,
            x.RequestPayload,
            x.ResponsePayload,
            x.Status.ToString(),
            x.CreatedAt
        );

    private static string EscapeJson(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
