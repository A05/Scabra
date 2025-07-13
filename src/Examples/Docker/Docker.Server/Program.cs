using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Scabra.Rpc.Server;
using Scabra.Observer.Publisher;

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
            
            var serverOptions = configuration.GetSection("scabra:rpc:server").Get<ScabraRpcServerOptions>();
            IScabraRpcServer rpcServer = new ScabraRpcServer(serverOptions, loggerFactory);

            var publisherOptions = configuration.GetSection("scabra:observer:publisher").Get<PublisherOptions>();
            IScabraObserverPublisher publisher = new ScabraObserverPublisher(publisherOptions, loggerFactory);

            rpcServer.RegisterService<ISomeService>(new SomeService());

            var examplePublisher = new ExamplePublisher(publisher);

            rpcServer.Start();
            publisher.Start();

            examplePublisher.StartPublishing();

            Console.CancelKeyPress += new ConsoleCancelEventHandler((_, e) =>
            {
                e.Cancel = true; // Prevent the OS from immediately terminating the process
                _cts.Cancel();
            });

            while (!_cts.Token.IsCancellationRequested)
                Thread.Sleep(500);

            examplePublisher.StopPublishing();

            rpcServer.Dispose();
            publisher.Dispose();
        }
    }
}
