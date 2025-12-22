namespace GaniPay.Accounting.Domain.Enums;

/// <summary>
/// DB column: direction (string). We store as "debit"/"credit".
/// </summary>
public enum AccountingDirection
{
    Debit = 1,
    Credit = 2
}
