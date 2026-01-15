namespace GaniPay.Workflow.API.ResultStore;

public sealed record TransferResult(
    bool Success,
    string Status,      // "Succeeded" | "Failed" | "Running"
    string Message,
    object? Data = null // Transfer'a özel payload (transactionId, sender/receiver balances vs)
);
