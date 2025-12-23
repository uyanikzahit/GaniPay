namespace GaniPay.Integration.Application.Contracts.Dtos;

public sealed record CallIntegrationResultDto(
    Guid LogId,
    string Status
);
