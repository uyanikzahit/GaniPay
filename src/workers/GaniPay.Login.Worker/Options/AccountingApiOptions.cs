namespace GaniPay.Login.Worker.Options;

public sealed class AccountingApiOptions
{
    public string BaseUrl { get; set; } = "http://host.docker.internal:5103";
}