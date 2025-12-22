namespace GaniPay.Payments.Application.Contracts.Dtos;

public sealed record WorkflowInstanceDto(
    long? WorkflowInstanceKey,
    string BpmnProcessId
);
