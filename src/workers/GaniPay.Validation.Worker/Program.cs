using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GaniPay.Validation.Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices(services =>
        {
            services.AddHostedService<WorkerHost>();
        });

        await builder.Build().RunAsync();
    }
}
