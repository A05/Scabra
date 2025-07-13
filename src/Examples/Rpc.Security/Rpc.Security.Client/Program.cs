using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Client.Hosting;
using System;

namespace Scabra.Examples.Rpc.Security
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
                .AddScabraRpcClient(builder.Configuration, new ExampleScabraSecurityHandler(null))
                .AddSingleton<ISecurityService>(sp => {
                    var channelProvider = sp.GetRequiredService<IScabraRpcChannelProvider>();
                    var channel = channelProvider.GetChannel("server");
                    return new SecurityServiceScabraProxy(channel);
                })
                .AddSingleton<ISomeProtectedService>(sp => {
                    var channelProvider = sp.GetRequiredService<IScabraRpcChannelProvider>();
                    var channel = channelProvider.GetChannel("server");
                    return new SomeProtectedServiceScabraProxy(channel);
                })
                .AddSingleton<SomeClient>();

            using var host = builder.Build();

            host.Start();

            var client = host.Services.GetRequiredService<SomeClient>();

            client.DemonstrateSecurityUsage();

            host.StopAsync().Wait();

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
