using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Scabra.Rpc.Server
{
    internal class CallQueue : MutexableQueue<Call>
    {
        const int InitialTimeoutInMs = 1;

        private readonly AutoResetEvent _are;

        public CallQueue(IEndpointLogger logger) : this(128, 1000, logger) { }

        public CallQueue(int length, int gainAccessTimeoutInMs, IEndpointLogger logger)
            : base(length, InitialTimeoutInMs, gainAccessTimeoutInMs, logger)
        {
            _are = new AutoResetEvent(false);
        }

        public void Enqueue(IEnumerable<byte[]> callFrames, int timeoutInMs)
        {
            Debug.Assert(callFrames != null, "Call frames is null.");
            Debug.Assert(timeoutInMs > 0, "Timeout is invalid.");

            Enqueue(call => call.Initialize(callFrames, timeoutInMs));

            _are.Set();
        }

        public bool TryDequeue(Call call)
        {
            return TryDequeue(call.CopyFrom);
        }

        public bool Wait(int timeoutInMs)
        {
            return _are.WaitOne(timeoutInMs);
        }

        public void Abort() 
        {
            GetAccessForAborting();

            try
            {
                foreach (var call in GetItems())
                    call.Abort();
            }
            finally
            {
                ReleaseAccess();
            }
        }

        private void GetAccessForAborting()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for aborting.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for aborting.";

            GainAccess(InitialTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }
    }
}