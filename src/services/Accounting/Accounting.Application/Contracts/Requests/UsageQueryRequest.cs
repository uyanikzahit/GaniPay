namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed record UsageQueryRequest(
    Guid CustomerId,
    string Currency,
    string Period,     // "day" | "month" | "year"
    string MetricType, // "amount" | "count"
    DateOnly Date
);
