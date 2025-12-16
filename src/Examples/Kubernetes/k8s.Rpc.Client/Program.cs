using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Client.Hosting;
using System;

namespace Scabra.Examples.k8s.Rpc
{
    class Program
    {
        private const int TimeoutInMs = 1000;

        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging
                .ClearProviders()
                .AddConsole();

            builder.Services
                .AddScabraRpcClient(builder.Configuration)
                .AddSingleton<IPingPongService>(sp => {
                    var channelProvider = sp.GetRequiredService<IScabraRpcChannelProvider>();
                    var channel = channelProvider.GetChannel("server");
                    return new PingPongServiceScabraProxy(channel);
                })
                .AddSingleton<PingPongClient>();

            using var host = builder.Build();
            host.Start();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var client = host.Services.GetRequiredService<PingPongClient>();
            client.Run();
                        
            if (!host.StopAsync().Wait(TimeoutInMs))
                logger.LogError("Timeout {Timeout} ms of host completion.", TimeoutInMs);

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
