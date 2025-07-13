using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Observer.Publisher.Hosting
{
    public class ScabraObserverPublisherHostedService : BackgroundService
    {
        private readonly IScabraObserverPublisher _publisher;

        public ScabraObserverPublisherHostedService(IScabraObserverPublisher publisher) 
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _publisher.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            _publisher.Dispose();
        }
    }
}
