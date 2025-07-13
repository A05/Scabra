using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Observer.Publisher;
using Scabra.Observer.Subscriber;
using System;
using System.Threading;

namespace Scabra.Benchmarking.Observer
{
    internal class Repeater : IRepeater
    {
        private readonly IScabraObserverPublisher _publisher;
        private readonly IScabraObserverSubscriber _terminatorSubscriber;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<Repeater> _logger;
        private bool _isReady;

        public Repeater(
            ITerminator terminatorProxy,
            IScabraObserverPublisher publisher,
            IScabraObserverSubscriber terminatorSubscriber,
            IHostApplicationLifetime lifetime,
            ILogger<Repeater> logger)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _terminatorSubscriber = terminatorSubscriber ?? throw new ArgumentNullException(nameof(terminatorSubscriber));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _lifetime.ApplicationStarted.Register(() =>
            {
                const int TerminatorReadinessTimeoutInMs = 60_000;

                if (WaitForTerminatorIsReady(terminatorProxy, TerminatorReadinessTimeoutInMs))
                {
                    _logger.LogInformation("Terminator is ready.");

                    _terminatorSubscriber.Subscribe<EmptyMessage>(topic: "empty", HandleEmptyMessage);
                    _terminatorSubscriber.Subscribe<ShutdownMessage>(topic: "shutdown", HandleShutdownMessage);

                    _isReady = true;

                    _logger.LogInformation("Set repeater as ready.");
                }
                else
                {
                    _logger.LogError("Terminator readiness timeout: {Timeout} ms.", TerminatorReadinessTimeoutInMs);

                    _lifetime.StopApplication();
                }
            });
        }

        public bool AreYouReady() => _isReady;

        private void HandleEmptyMessage(EmptyMessage message)
        {
            _publisher.Publish("empty", message);
        }

        private void HandleShutdownMessage(ShutdownMessage message)
        {
            _logger.LogInformation("Shutdown messsage received.");

            _lifetime.StopApplication();
        }

        private bool WaitForTerminatorIsReady(ITerminator terminatorProxy, int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (true)
            {
                try 
                { 
                    if (terminatorProxy.AreYouReady()) 
                        break; 
                } 
                catch (Exception ex)
                {
                    _logger.LogInformation("Terminator is not ready: {Message}", ex.Message);
                }

                if (Environment.TickCount - start >= timeoutInMs)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }
    }
}