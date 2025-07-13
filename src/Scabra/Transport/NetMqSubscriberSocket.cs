using NetMQ.Sockets;

namespace Scabra
{
    public class NetMqSubscriberSocket : NetMqSocket
    {
        private readonly SubscriberSocket _socket;

        internal NetMqSubscriberSocket(SubscriberSocket socket) : base(socket)
        {
            _socket = socket;
        }

        public void Subscribe(string topic)
        {
            _socket.Subscribe(topic);
        }

        public void Unsubscribe(string topic)
        {
            _socket.Unsubscribe(topic);
        }
    }
}
