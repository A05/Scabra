using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Scabra.Rpc;
using Scabra.Rpc.Client;
using Scabra.Observer.Subscriber;

namespace Scabra.Examples.Docker
{
    class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        static void Main()
        {
            var loggerFactory = LoggerFactory.Create(
                builder => {
                    builder.AddConsole();
                });

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var rpcChannelAddress = configuration.GetSection("scabra:rpc:clients:0:address").Value;
            var rpcChannelOptions = new ScabraRpcChannelOptions() { Address = rpcChannelAddress };
            var rpcChannel = new ScabraRpcChannel(rpcChannelOptions, new NullScabraSecurityHandler(), loggerFactory);

            var publisherAddress = configuration.GetSection("scabra:observer:subscribers:0:address").Value;
            var subscriberOptions = new ScabraObserverSubscriberOptions() { Address = publisherAddress };
            var subscriber = new ScabraObserverSubscriber(subscriberOptions, loggerFactory);

            var client = new SomeClient(rpcChannel, subscriber);

            client.StartRemoteJob();

            Console.CancelKeyPress += new ConsoleCancelEventHandler((_, e) => 
            {
                e.Cancel = true; // Prevent the OS from immediately terminating the process
                _cts.Cancel();
            });

            while (!_cts.Token.IsCancellationRequested)
                Thread.Sleep(500);

            client.StopRemoteJob();

            rpcChannel.Dispose();
            subscriber.Dispose();
        }
    }
}
