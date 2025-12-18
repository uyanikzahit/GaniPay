namespace GaniPay.TransactionLimit.Application.Contracts.Requests;

public sealed record CreateLimitDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    string Period,     // day/month/year (string)
    string MetricType, // amount/count/balance (string)
    bool IsVisible = true
);
