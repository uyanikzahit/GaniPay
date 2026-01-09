namespace GaniPay.Transfer.Worker.Options;

public sealed class AccountingApiOptions
{
    public const string SectionName = "AccountingApi";

    public string BaseUrl { get; set; } = "http://localhost:5103";
}