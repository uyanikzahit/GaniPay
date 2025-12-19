namespace GaniPay.Expense.Domain.Entities;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
