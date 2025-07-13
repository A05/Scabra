using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Client.Hosting;
using System;

namespace Scabra.Examples.Rpc
{
    class Program
    {
        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging
                .ClearProviders()
                .AddConsole();

            builder.Services
                .AddScabraRpcClient(builder.Configuration)
                .AddSingleton<ISomeService>(sp => {
                    var channelProvider = sp.GetRequiredService<IScabraRpcChannelProvider>();
                    var channel = channelProvider.GetChannel("server");
                    return new SomeServiceScabraProxy(channel);
                })
                .AddSingleton<SomeClient>();

            using var host = builder.Build();

            host.Start();

            var client = host.Services.GetRequiredService<SomeClient>();

            client.DoRemoteProcedureCalls();

            host.StopAsync().Wait();

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
