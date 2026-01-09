namespace GaniPay.Transfer.Worker.Options;

public sealed class ZeebeOptions
{
    public const string SectionName = "Zeebe";

    public string GatewayAddress { get; set; } = "127.0.0.1:26500";
}
