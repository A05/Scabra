using System;

namespace Scabra.Observer.Subscriber
{
    public interface IScabraObserverSubscriber : IDisposable
    {
        void Subscribe<TMessage>(string topic, Action<TMessage> handler);

        void Unsubscribe<TMessage>(string topic, Action<TMessage> handler);
    }
}
