using GaniPay.TransactionLimit.Domain.Enums;

namespace GaniPay.TransactionLimit.Domain.Entities;

public sealed class CustomerLimit
{
    public Guid Id { get; set; }

    // Logical FK -> customer.id (Customer domain)
    public Guid CustomerId { get; set; }

    public Guid LimitDefinitionId { get; set; }
    public LimitDefinition? LimitDefinition { get; set; }

    // Period breakdown (opsiyonel)
    public short? Year { get; set; }
    public short? Month { get; set; }
    public short? Day { get; set; }

    public decimal Value { get; set; }
    public string? Currency { get; set; }

    public LimitSource Source { get; set; } = LimitSource.System;
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
