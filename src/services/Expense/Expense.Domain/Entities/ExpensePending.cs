using GaniPay.Expense.Domain.Enums;

namespace GaniPay.Expense.Domain.Entities;

public sealed class ExpensePending : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid AccountingTxId { get; private set; }
    public Guid ExpenseId { get; private set; }

    public decimal CalculatedAmount { get; private set; }
    public string Currency { get; private set; } = "TRY";

    public ExpensePendingStatus PendingStatus { get; private set; } = ExpensePendingStatus.Pending;

    public DateTime TransactionDate { get; private set; }
    public short TryCount { get; private set; }
    public string? ResultCode { get; private set; }

    private ExpensePending() { }

    public ExpensePending(
        Guid id,
        Guid accountingTxId,
        Guid expenseId,
        decimal calculatedAmount,
        string currency,
        DateTime transactionDateUtc)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        if (accountingTxId == Guid.Empty) throw new ArgumentException("AccountingTxId boþ olamaz.", nameof(accountingTxId));
        if (expenseId == Guid.Empty) throw new ArgumentException("ExpenseId boþ olamaz.", nameof(expenseId));
        if (calculatedAmount < 0) throw new ArgumentOutOfRangeException(nameof(calculatedAmount), "CalculatedAmount negatif olamaz.");

        AccountingTxId = accountingTxId;
        ExpenseId = expenseId;
        CalculatedAmount = calculatedAmount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "TRY" : currency.Trim().ToUpperInvariant();

        TransactionDate = transactionDateUtc.Kind == DateTimeKind.Utc ? transactionDateUtc : DateTime.SpecifyKind(transactionDateUtc, DateTimeKind.Utc);

        PendingStatus = ExpensePendingStatus.Pending;
        TryCount = 0;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded()
    {
        PendingStatus = ExpensePendingStatus.Succeeded;
        Touch();
    }

    public void MarkFailed(string? resultCode)
    {
        PendingStatus = ExpensePendingStatus.Failed;
        ResultCode = resultCode;
        Touch();
    }

    public void IncrementTry(string? resultCode = null)
    {
        if (TryCount < short.MaxValue) TryCount++;
        ResultCode = resultCode;
        Touch();
    }
}
