using System;

namespace Scabra
{
    public class EndpointCrashedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public EndpointCrashedEventArgs(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
    }

    public delegate void EndpointCrashedEventHandler(object sender, EndpointCrashedEventArgs args);
}
