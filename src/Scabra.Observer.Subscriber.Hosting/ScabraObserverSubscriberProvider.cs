using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Scabra.Observer.Subscriber
{
    internal class ScabraObserverSubscriberProvider : IScabraObserverSubscriberProvider
    {
        private readonly (string name, IScabraObserverSubscriber subscriber)[] _subscribers;

        internal ScabraObserverSubscriberProvider(IConfigurationSection configuration)
        {
            var options = configuration.Get<SubscribersOptions>();

            _subscribers = new (string, IScabraObserverSubscriber)[options.Subscribers.Length];

            for (int i = 0; i < options.Subscribers.Length; i++)
            {
                var s = options.Subscribers[i];

                var subscriberOptions = new ScabraObserverSubscriberOptions() { Address = s.Address };
                var subscriber = new ScabraObserverSubscriber(subscriberOptions, NullLoggerFactory.Instance);

                _subscribers[i] = (s.Name, subscriber);
            }
        }

        public IScabraObserverSubscriber GetSubscriber(string name)
        {
            foreach ((string name, IScabraObserverSubscriber subscriber) i in _subscribers)
                if (i.name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return i.subscriber;

            throw new ArgumentOutOfRangeException(nameof(name), $"'{name}' subscriber is not found.");
        }
    }
}
