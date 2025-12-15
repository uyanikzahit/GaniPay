using System;
using System.Threading;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

const string GatewayAddress = "127.0.0.1:26500";
const string JobType = "wallet.topup.hello";

var client = ZeebeClient.Builder()
    .UseGatewayAddress(GatewayAddress)
    .UsePlainText()
    .Build();

Console.WriteLine($"🔌 Connecting to Zeebe: {GatewayAddress}");

var topology = client.TopologyRequest().Send().GetAwaiter().GetResult();
Console.WriteLine($"✅ Topology: {topology}");

using var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

client.NewWorker()
    .JobType(JobType)
    .Handler((IJobClient jobClient, IJob job) =>
    {
        Console.WriteLine($"📥 Job received. key={job.Key}, processInstanceKey={job.ProcessInstanceKey}");

        jobClient.NewCompleteJobCommand(job.Key)
            .Send()
            .GetAwaiter()
            .GetResult();

        Console.WriteLine($"✅ Job completed. key={job.Key}");
    })
    .MaxJobsActive(5)
    .Name("ganipay-wallet-topup-hello-worker")
    .PollInterval(TimeSpan.FromSeconds(1))
    .Timeout(TimeSpan.FromSeconds(30))
    .Open();

Console.WriteLine($"🚀 Worker started for jobType: {JobType}");
waitHandle.WaitOne();
