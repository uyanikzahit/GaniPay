namespace GaniPay.Payments.Domain.Enums;

public enum PaymentStatus : short
{
    Initiated = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4
}
