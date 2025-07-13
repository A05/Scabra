using System;

namespace Scabra.Rpc.Server
{
    public class ScabraRpcServerCrashedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ScabraRpcServerCrashedEventArgs(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
    }

    public delegate void ScabraRpcServerCrashedEventHandler(object sender, ScabraRpcServerCrashedEventArgs args);
}
