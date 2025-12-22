namespace GaniPay.Payments.Application.Abstractions;

// Camunda/Zeebe client wrapper.
// MVP’de Noop implementasyonla çalýþýr, sonra Zeebe eklenir.
public interface IWorkflowClient
{
    Task<long?> StartPaymentWorkflowAsync(string bpmnProcessId, object variables, CancellationToken ct = default);
}
