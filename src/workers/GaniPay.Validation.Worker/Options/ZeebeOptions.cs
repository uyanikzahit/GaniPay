namespace GaniPay.Validation.Worker.Options;

public sealed class ZeebeOptions
{
    public string GatewayAddress { get; set; } = "127.0.0.1:26500";
    public string WorkerName { get; set; } = "ganipay-validation-worker";
    public int DefaultTimeoutSeconds { get; set; } = 120;
}
