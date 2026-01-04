namespace GaniPay.TopUp.Worker.Options;

public sealed class ZeebeOptions
{
    public string GatewayAddress { get; set; } = "127.0.0.1:26500";
}
