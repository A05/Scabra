using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Scabra.Examples.k8s.Rpc
{
    internal class PingPongClient
    {
        private readonly IPingPongService _service;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;

        private int _requestCount, _errorCount;

        public PingPongClient(IPingPongService service, IHostApplicationLifetime lifetime, ILogger<PingPongClient> logger) 
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run()
        {
            var hostname = Dns.GetHostName();

            _logger.LogInformation("Host name is {Hostname}.", hostname);

            while (!_lifetime.ApplicationStopping.IsCancellationRequested)
            {
                try
                {
                    var reply = _service.PingPong(hostname);
                    if (reply != hostname)
                        _logger.LogError("Reply does not match: {Reply}.", reply);
                }
                catch (Exception ex)
                {
                    _errorCount++;
                    _logger.LogError(ex, "Failed to ping pong.");
                }
                finally
                {
                    if (++_requestCount % 5_000 == 0)
                        _logger.LogInformation("Requests/Errors = {RequestCount}/{ErrorCount}", _requestCount, _errorCount);
                }
            }
        }
    }
}
