using GaniPay.Payments.Domain.Enums;

namespace GaniPay.Payments.Domain.Entities;

public sealed class PaymentProcess
{
    public Guid Id { get; set; }

    // Dýþ dünyaya döndüðümüz izleme id’si
    public string CorrelationId { get; set; } = default!;

    // Ayný isteðin tekrarýný double iþlem yapmadan yönetmek
    public string IdempotencyKey { get; set; } = default!;

    public Guid CustomerId { get; set; }

    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Camunda/Zeebe instance key
    public long? WorkflowInstanceKey { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
