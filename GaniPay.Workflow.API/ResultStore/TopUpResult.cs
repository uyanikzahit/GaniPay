namespace GaniPay.Workflow.API.ResultStore;

public sealed record TopUpResult(
    bool Success,
    string Status,     // "Succeeded" | "Failed" | "Running"
    string Message,
    object? Data = null // Topup'a özel payload (balance, transactionId vs)
);
