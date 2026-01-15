namespace GaniPay.Workflow.API.ResultStore;

public sealed record LoginResult(
    bool Success,
    string Status,   // "Succeeded" | "Failed" | "Running"
    string Message,
    string? Token = null,

    // ✅ ekstra alanlar (akıştan gelen data)
    string? CustomerId = null,
    object? Customer = null,
    object? Wallets = null
);