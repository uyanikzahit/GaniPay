using GaniPay.Accounting.Application.Contracts.Enums;

namespace GaniPay.Accounting.Application.Contracts.Requests;

public sealed class BookTransactionRequest
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal Amount { get; set; }

    public AccountingOperationType OperationType { get; set; }

    // transfer-0001 gibi business id (Payments/Workflow üretir)
    public string ReferenceId { get; set; } = default!;

    // aynı request tekrar gelirse duplicate booking engeller
    public string IdempotencyKey { get; set; } = default!;

    // workflow korelasyon
    public string CorrelationId { get; set; } = default!;

    public string? Description { get; set; }
}
