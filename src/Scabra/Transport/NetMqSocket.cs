using System;
using NetMQ;

namespace Scabra
{
    public abstract class NetMqSocket : IDisposable
    {
        //
        // TODO: SR (NU) See if 5000 is enough. It is experimental value.
        //
        private static readonly TimeSpan SendingTimeoutInMilliseconds = TimeSpan.FromMilliseconds(5000);

        private bool _disposed = false;
        private readonly NetMQSocket _socket;

        protected NetMqSocket(NetMQSocket socket)
        {
            _socket = socket;
        }

        public void Connect(string address)
        {
            _socket.Connect(address);
        }
        public void Bind(string address)
        {
            _socket.Bind(address);
        }
        public bool HasIn
        {
            get { return _socket.HasIn; }
        }

        public byte[] Receive(out bool hasMore)
        {
            if (_socket.TryReceiveFrameBytes(TimeSpan.Zero, out var bytes, out hasMore))
                return bytes;

            return null;
        }

        public string ReceiveString(out bool hasMore)
        {
            if (_socket.TryReceiveFrameString(TimeSpan.Zero, out var frame, out hasMore))
                return frame;

            return null;
        }
 
        public bool HasOut
        {
            get { return _socket.HasOut; }
        }

        public void Send(byte[] message)
        {
            if (!_socket.TrySendFrame(SendingTimeoutInMilliseconds, message, message.Length))
                throw new TimeoutException();
        }

        public void SendMore(byte[] message)
        {
            if (!_socket.TrySendFrame(SendingTimeoutInMilliseconds, message, message.Length, more: true))
                throw new TimeoutException();
        }

        public void SendMore(string message)
        {
            if (!_socket.TrySendFrame(SendingTimeoutInMilliseconds, message, more: true))
                throw new TimeoutException();
        }

        #region IDisposible

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _socket.Dispose();
                // Free any other managed objects here. 
                //
            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;
        }

        #endregion
    }
}
