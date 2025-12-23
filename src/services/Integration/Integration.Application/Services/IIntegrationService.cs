using GaniPay.Integration.Application.Contracts.Dtos;
using GaniPay.Integration.Application.Contracts.Requests;

namespace GaniPay.Integration.Application.Services;

public interface IIntegrationService
{
    Task<CallIntegrationResultDto> CallAsync(CallIntegrationRequest request, CancellationToken ct);
    Task<IntegrationLogDto> GetAsync(GetIntegrationLogRequest request, CancellationToken ct);
    Task<List<IntegrationLogDto>> GetProviderLogsAsync(GetProviderLogsRequest request, CancellationToken ct);
}
