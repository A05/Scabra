using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra.Rpc
{
    public abstract class Mutexable
    {
        private readonly int _maxTimeoutInMs;
        private readonly SemaphoreSlim _semaphore;
        private readonly IEndpointLogger _logger;

        protected Mutexable(int maxTimeoutInMs, IEndpointLogger logger) 
        {
            _maxTimeoutInMs = maxTimeoutInMs;
            _logger = logger;

            _semaphore = new SemaphoreSlim(1, 1);
        }

        protected void GainAccess(int initialeTimeoutInMs, CancellationToken cToken, Func<string> getWarningMessage, Func<string> getExceptionMessage)
        {
            Debug.Assert(initialeTimeoutInMs < _maxTimeoutInMs);

            int waitingTime = 0;
            string warningMessage = null;

            for (int timeout = initialeTimeoutInMs; true; timeout *= 2)
            {
                if (_semaphore.Wait(timeout, cToken))
                    return;

                warningMessage ??= getWarningMessage();
                _logger.LogWarning(warningMessage, timeout);

                if ((waitingTime += timeout) >= _maxTimeoutInMs)
                {
                    var exceptionMessage = getExceptionMessage();
                    throw new TimeoutException(string.Format(exceptionMessage, timeout));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReleaseAccess()
        {
            _semaphore.Release();
        }
    }
}
