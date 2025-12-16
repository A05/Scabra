using Microsoft.Extensions.Logging;
using System;

namespace Scabra.Examples.k8s.Rpc
{
    public class PingPongService : IPingPongService
    {
        private int _requestCount;

        private readonly ILogger<PingPongService> _logger;
        
        public PingPongService(ILogger<PingPongService> logger) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string PingPong(string arg)
        {
            if (++_requestCount % 5_000 == 0)
                _logger.LogInformation("Request count: {Count}.", _requestCount);

            return arg;
        }
    }
}