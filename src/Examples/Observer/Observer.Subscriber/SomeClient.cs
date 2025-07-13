using System;
using Scabra.Observer.Subscriber;

namespace Scabra.Examples.Observer
{
    internal class SomeClient : IDisposable
    {
        private readonly IScabraObserverSubscriber _subscriber;

        public SomeClient(IScabraObserverSubscriber subscriber) 
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

            _subscriber.Subscribe<MessageOfTopicA>("topic_a", HandleMessageOfTopicA);
            _subscriber.Subscribe<MessageOfTopicB>("topic_b", HandleMessageOfTopicB);
            _subscriber.Subscribe<MessageOfTopicC>("", HandleMessageOfTopicC);
        }

        public void Dispose()
        {
            _subscriber.Unsubscribe<MessageOfTopicA>("topic_a", HandleMessageOfTopicA);
            _subscriber.Unsubscribe<MessageOfTopicB>("topic_b", HandleMessageOfTopicB);
            _subscriber.Unsubscribe<MessageOfTopicC>("", HandleMessageOfTopicC);
        }

        private void HandleMessageOfTopicA(MessageOfTopicA message)
        {
            Console.WriteLine($"{message} handled.");
        }

        private void HandleMessageOfTopicB(MessageOfTopicB message)
        {
            Console.WriteLine($"{message} handled.");
        }

        private void HandleMessageOfTopicC(MessageOfTopicC message)
        {
            Console.WriteLine($"{message} handled.");
        }
    }
}
