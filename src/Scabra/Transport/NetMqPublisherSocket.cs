using NetMQ.Sockets;

namespace Scabra
{
    public class NetMqPublisherSocket : NetMqSocket
    {
        internal NetMqPublisherSocket(PublisherSocket socket) : base(socket)
        {
        }
    }
}
