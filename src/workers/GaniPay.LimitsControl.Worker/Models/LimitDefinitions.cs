namespace GaniPay.LimitsControl.Worker.Models;

public sealed class LimitDefinitions
{
    public decimal SingleMax { get; set; }
    public decimal DailyMax { get; set; }
}