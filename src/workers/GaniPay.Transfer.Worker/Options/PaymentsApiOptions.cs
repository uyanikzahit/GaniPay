namespace GaniPay.Transfer.Worker.Options;

public sealed class PaymentsApiOptions
{
    public const string SectionName = "PaymentsApi";

    public string BaseUrl { get; set; } = "http://localhost:5104";
}