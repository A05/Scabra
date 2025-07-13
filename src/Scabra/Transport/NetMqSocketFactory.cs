using NetMQ.Sockets;

namespace Scabra
{
    public class NetMqSocketFactory
    {
        public NetMqRouterSocket CreateRouterSocket()
        {
            return new NetMqRouterSocket(new RouterSocket());
        }

        public NetMqDealerSocket CreateDealerSocket()
        {
            return new NetMqDealerSocket(new DealerSocket());
        }

        public NetMqPublisherSocket CreatePublisherSocket()
        {
            return new NetMqPublisherSocket(new PublisherSocket());
        }

        public NetMqSubscriberSocket CreateSubscriberSocket()
        {
            return new NetMqSubscriberSocket(new SubscriberSocket());
        }
    }
}
