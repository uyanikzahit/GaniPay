namespace GaniPay.LimitsControl.Worker.Options;

public sealed class ServiceEndpointsOptions
{
    public string AccountingBaseUrl { get; set; } = "http://localhost:5103";
    public string LimitsBaseUrl { get; set; } = "http://localhost:5104";
}