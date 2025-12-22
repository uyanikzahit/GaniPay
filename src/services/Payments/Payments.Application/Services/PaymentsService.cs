using GaniPay.Payments.Application.Abstractions;
using GaniPay.Payments.Application.Abstractions.Repositories;
using GaniPay.Payments.Application.Contracts.Dtos;
using GaniPay.Payments.Application.Contracts.Requests;
using GaniPay.Payments.Domain.Entities;
using GaniPay.Payments.Domain.Enums;

namespace GaniPay.Payments.Application.Services;

public sealed class PaymentsService : IPaymentsService
{
    private readonly IPaymentProcessRepository _repo;
    private readonly IWorkflowClient _workflow;
    private readonly IUnitOfWork _uow;

    public PaymentsService(
        IPaymentProcessRepository repo,
        IWorkflowClient workflow,
        IUnitOfWork uow)
    {
        _repo = repo;
        _workflow = workflow;
        _uow = uow;
    }

    public async Task<StartPaymentResultDto> StartTransferAsync(StartTransferRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new InvalidOperationException("idempotencyKey is required");

        if (request.Amount <= 0)
            throw new InvalidOperationException("amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("currency is required");

        if (string.IsNullOrWhiteSpace(request.TargetIban))
            throw new InvalidOperationException("targetIban is required");

        // 1) Idempotency
        var existing = await _repo.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
            return new StartPaymentResultDto(existing.CorrelationId, existing.Status.ToString());

        // 2) Create process state
        var process = new PaymentProcess
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            IdempotencyKey = request.IdempotencyKey,
            CustomerId = request.CustomerId,
            Type = PaymentType.Transfer,
            Status = PaymentStatus.Running,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _repo.AddAsync(process, ct);
        await _uow.SaveChangesAsync(ct);

        // 3) Start workflow (MVP: Noop -> null key)
        var variables = new
        {
            process.CorrelationId,
            process.CustomerId,
            process.Amount,
            process.Currency,
            request.TargetIban,
            TransferType = request.TransferType.ToString()
        };

        var key = await _workflow.StartPaymentWorkflowAsync("wallet.transfer", variables, ct);

        process.WorkflowInstanceKey = key;
        process.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(process, ct);
        await _uow.SaveChangesAsync(ct);

        return new StartPaymentResultDto(process.CorrelationId, process.Status.ToString());
    }

    public async Task<StartPaymentResultDto> StartTopUpAsync(StartTopUpRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new InvalidOperationException("idempotencyKey is required");

        if (request.Amount <= 0)
            throw new InvalidOperationException("amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidOperationException("currency is required");

        // 1) Idempotency
        var existing = await _repo.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
            return new StartPaymentResultDto(existing.CorrelationId, existing.Status.ToString());

        // 2) Create process state
        var process = new PaymentProcess
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            IdempotencyKey = request.IdempotencyKey,
            CustomerId = request.CustomerId,
            Type = PaymentType.TopUp,
            Status = PaymentStatus.Running,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _repo.AddAsync(process, ct);
        await _uow.SaveChangesAsync(ct);

        // 3) Start workflow (MVP: Noop -> null key)
        var variables = new
        {
            process.CorrelationId,
            process.CustomerId,
            process.Amount,
            process.Currency
        };

        var key = await _workflow.StartPaymentWorkflowAsync("wallet.topup", variables, ct);

        process.WorkflowInstanceKey = key;
        process.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(process, ct);
        await _uow.SaveChangesAsync(ct);

        return new StartPaymentResultDto(process.CorrelationId, process.Status.ToString());
    }

    public async Task<PaymentProcessDto> GetStatusAsync(GetPaymentStatusRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CorrelationId))
            throw new InvalidOperationException("correlationId is required");

        var process = await _repo.GetByCorrelationIdAsync(request.CorrelationId, ct)
                      ?? throw new InvalidOperationException("payment not found");

        return new PaymentProcessDto(
            process.Id,
            process.CorrelationId,
            process.CustomerId,
            process.Type.ToString(),
            process.Status.ToString(),
            process.Amount,
            process.Currency,
            process.WorkflowInstanceKey,
            process.ErrorCode,
            process.ErrorMessage,
            process.CreatedAt,
            process.UpdatedAt
        );
    }
}
