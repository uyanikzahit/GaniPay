namespace GaniPay.Accounting.Domain.Enums;

public enum OperationType : short
{
    TopUp = 1,
    TransferOut = 2,
    TransferIn = 3,
    Fee = 4,
    Adjustment = 5
}
