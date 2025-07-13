using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc.Server.Hosting
{
    public class ScabraRpcServerHostedService : BackgroundService
    {
        private readonly IScabraRpcServer _server;

        public ScabraRpcServerHostedService(IScabraRpcServer server) 
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _server.Start();

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

            _server.Dispose();
        }
    }
}
