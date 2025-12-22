namespace GaniPay.Accounting.Domain.Enums;

/// <summary>
/// DB column: operation_type (int).
/// Bu enum deðerlerini MVP’de sabitliyoruz. Geniþletilebilir.
/// </summary>
public enum AccountingOperationType : int
{
    TopUp = 1,
    TransferOut = 2,
    TransferIn = 3,
    Fee = 4,
    Adjustment = 5
}
