using NetMQ.Sockets;

namespace Scabra
{
    public class NetMqRouterSocket : NetMqSocket
    {
        internal NetMqRouterSocket(RouterSocket socket) : base(socket)
        {
        }
    }
}
