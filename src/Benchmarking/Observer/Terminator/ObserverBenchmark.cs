using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scabra.Observer.Publisher;
using Scabra.Observer.Subscriber;
using Scabra.Rpc.Client;
using Scabra.Rpc.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Benchmarking.Observer
{
    public class ObserverBenchmark
    {
        private IScabraObserverSubscriber _repeaterSubscriber;
        private IScabraObserverPublisher _publisher;
        private IScabraRpcChannel _rpcChannel;
        private IScabraRpcServer _rpcServer;
        private AutoResetEvent _repeaterRepliedEvent;

        [GlobalSetup]
        public void Setup()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders().AddConsole();
            });

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            var observerConfiguration = configuration.GetSection("scabra:observer");

            var publisherAddress = observerConfiguration.GetSection("subscribers:0:address").Value;
            var subscriberOptions = new ScabraObserverSubscriberOptions() { Address = publisherAddress };
            _repeaterSubscriber = new ScabraObserverSubscriber(subscriberOptions, loggerFactory);

            var publisherOptions = observerConfiguration.GetSection("publisher").Get<PublisherOptions>();
            _publisher = new ScabraObserverPublisher(publisherOptions, loggerFactory);

            var rpcConfiguration = configuration.GetSection("scabra:rpc");

            var repeaterAddress = rpcConfiguration.GetSection("clients:0:address").Value;
            var rpcChannelOptions = new ScabraRpcChannelOptions() { Address = repeaterAddress };
            _rpcChannel = new ScabraRpcChannel(rpcChannelOptions);
                        
            var serverOptions = rpcConfiguration.GetSection("server").Get<ScabraRpcServerOptions>();
            _rpcServer = new ScabraRpcServer(serverOptions, loggerFactory);
            _rpcServer.Crashed += (_, args) =>
            {
                Console.WriteLine(args.Exception);
            };

            var terminator = new Terminator();
            _rpcServer.RegisterService<ITerminator>(terminator);

            var repeaterProxy = new RepeaterProxy(_rpcChannel);

            _publisher.Start();
            _rpcServer.Start();

            terminator._isReady = true;

            Console.WriteLine("Set terminator as ready.");

            var repeaterWaiting = Task.Run(() =>
            {
                const int RepeaterReadinessTimeoutInMs = 60_000;
                if (!WaitForRepeaterIsReady(repeaterProxy, RepeaterReadinessTimeoutInMs))
                    throw new Exception($"Repeater readiness timeout: {RepeaterReadinessTimeoutInMs} ms.");
            });

            repeaterWaiting.Wait();

            Console.WriteLine("Repeater is ready.");

            _repeaterRepliedEvent = new AutoResetEvent(false);

            _repeaterSubscriber.Subscribe<EmptyMessage>("empty", HandleEmptyMessage);
        }

        [Benchmark]
        public void Empty_Message_Observer()
        {
            // The latency measurement results should be divided roughly by 2 because
            // a message is sent to the repeater that resends the message back to the terminal.

            _publisher.Publish("empty", new EmptyMessage());

            if (!_repeaterRepliedEvent.WaitOne(1_000))
                throw new TimeoutException("The timeout occurred while waiting for a reply from the repeater.");
        }

        private void HandleEmptyMessage(EmptyMessage message)
        {
            _repeaterRepliedEvent.Set();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _publisher.Publish("shutdown", new ShutdownMessage());

            _publisher.Dispose();
            _repeaterSubscriber.Dispose();
            _rpcChannel.Dispose();
            _rpcServer.Dispose();
        }

        private bool WaitForRepeaterIsReady(IRepeater repeaterProxy, int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (true)
            {
                try 
                { 
                    if (repeaterProxy.AreYouReady()) 
                        break; 
                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"Repeater is not ready: {ex.Message}");
                }

                if (Environment.TickCount - start >= timeoutInMs)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }
    }
}
