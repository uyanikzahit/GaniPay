using GaniPay.Payments.Application.Abstractions;

namespace GaniPay.Payments.Infrastructure.Workflow;

public sealed class NoopWorkflowClient : IWorkflowClient
{
    public Task<long?> StartPaymentWorkflowAsync(string bpmnProcessId, object variables, CancellationToken ct = default)
    {

        // Þu an sadece Payments API + DB akýþýný test etmek için null dönüyoruz.
        return Task.FromResult<long?>(null);
    }
}
