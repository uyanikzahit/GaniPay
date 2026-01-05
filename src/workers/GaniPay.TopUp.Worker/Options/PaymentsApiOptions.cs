namespace GaniPay.TopUp.Worker.Options;

public sealed class PaymentsApiOptions
{
    public const string SectionName = "Apis:Payments";
    public string BaseUrl { get; set; } = "http://localhost:7241";
}
