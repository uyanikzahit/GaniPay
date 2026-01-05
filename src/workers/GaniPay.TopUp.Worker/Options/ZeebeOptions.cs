namespace GaniPay.TopUp.Worker.Options;

public sealed class ZeebeOptions
{
    public const string SectionName = "Zeebe";

    public string GatewayAddress { get; set; } = "localhost:26500";

    // Local dev için genelde plaintext kullanılır.
    public bool UsePlainText { get; set; } = true;

    // İstersen ileride Camunda Cloud için ek alanlar koyarsın (ClientId/Secret vb.)
}
