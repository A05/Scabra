using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Observer.Publisher;
using Scabra.Observer.Publisher.Hosting;
using Scabra.Observer.Subscriber;
using Scabra.Observer.Subscriber.Hosting;
using Scabra.Rpc.Client.Hosting;
using Scabra.Rpc.Server;
using Scabra.Rpc.Server.Hosting;

namespace Scabra.Benchmarking.Observer
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
                .AddScabraRpcServer(builder.Configuration)
                .AddScabraObserverPublisher(builder.Configuration)
                .AddScabraObserverSubscriber(builder.Configuration)                
                .AddSingleton<ITerminator>(sp =>
                {
                    var provider = sp.GetRequiredService<IScabraRpcChannelProvider>();
                    var channel = provider.GetChannel("terminator");
                    return new TerminatorProxy(channel);
                });

            using var host = builder.Build();

            var terminatorProxy = host.Services.GetRequiredService<ITerminator>();
            var publisher = host.Services.GetRequiredService<IScabraObserverPublisher>();
            var subscriberProvider = host.Services.GetRequiredService<IScabraObserverSubscriberProvider>();
            var subscriber = subscriberProvider.GetSubscriber("terminator");
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var logger = host.Services.GetRequiredService<ILogger<Repeater>>();

            var repeater = new Repeater(terminatorProxy, publisher, subscriber, lifetime, logger);

            var scabraServer = host.Services.GetRequiredService<IScabraRpcServer>();
            scabraServer.RegisterService<IRepeater>(repeater);

            host.Run();
        }
    }
}
