namespace GaniPay.DataCreation.Worker.Options;

public sealed class WorkerOptions
{
    public string GatewayAddress { get; set; } = "127.0.0.1:26500";
    public string WorkerName { get; set; } = "ganipay-data-creation-worker";

    public int MaxJobsActive { get; set; } = 32;
    public int TimeoutSeconds { get; set; } = 60;
    public int PollIntervalMs { get; set; } = 200;

    public int Retries { get; set; } = 3;
}
