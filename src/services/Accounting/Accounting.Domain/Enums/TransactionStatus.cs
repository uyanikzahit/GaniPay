namespace GaniPay.Accounting.Domain.Enums;

public enum TransactionStatus : short
{
    Pending = 1,
    Booked = 2,
    Reversed = 3,
    Failed = 4
}
