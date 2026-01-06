namespace GaniPay.LimitsControl.Worker.Models;

public sealed class CustomerLimits
{
    public decimal DailyUsed { get; set; }
    public decimal DailyRemaining { get; set; }
}