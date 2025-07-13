using System;

namespace Scabra.Observer.Publisher
{
    public interface IScabraObserverPublisher : IDisposable
    {
        void Start();

        void Publish<TMessage>(string topic, TMessage message);
    }
}
