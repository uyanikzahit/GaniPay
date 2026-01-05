namespace GaniPay.TopUp.Worker.Options;

public sealed class AccountingApiOptions
{
    public const string SectionName = "Apis:Accounting";
    public string BaseUrl { get; set; } = "http://localhost:5103";
}
