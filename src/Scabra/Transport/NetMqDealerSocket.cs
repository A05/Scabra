using NetMQ.Sockets;

namespace Scabra
{
    public class NetMqDealerSocket : NetMqSocket
    {        
        internal NetMqDealerSocket(DealerSocket socket) : base(socket)
        {
        }
    }
}
