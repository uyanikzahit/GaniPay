namespace GaniPay.Integration.Application.Contracts.Dtos;

public sealed record IntegrationLogDto(
    Guid Id,
    Guid ProviderId,
    string Operation,
    string RequestPayload,
    string ResponsePayload,
    string Status,
    DateTime CreatedAt
);
