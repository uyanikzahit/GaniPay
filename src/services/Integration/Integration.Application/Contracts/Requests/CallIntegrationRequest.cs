namespace GaniPay.Integration.Application.Contracts.Requests;

public sealed record CallIntegrationRequest(
    string ProviderCode,
    string Operation,
    string RequestPayload
);
