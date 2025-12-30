namespace GaniPay.Validation.Worker.Options;

public sealed class ApiOptions
{
    public string CustomerBaseUrl { get; set; } = "http://localhost:5101";
    public string IntegrationBaseUrl { get; set; } = "http://localhost:5094";
}
