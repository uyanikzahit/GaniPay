namespace GaniPay.TopUp.Worker.Models;

public sealed class PaymentsInitiateTopUpResponse
{
    public string CorrelationId { get; set; } = default!;
    public string Status { get; set; } = default!; // "Running" vs
}