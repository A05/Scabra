using System;
using System.Threading.Tasks;
using Scabra.Observer.Publisher;

namespace Scabra.Examples.Docker
{
    internal class ExamplePublisher
    {
        private readonly IScabraObserverPublisher _publisher;
        private readonly Random _rand = new();

        private bool _isStopping;
        private Task _publishing;

        public ExamplePublisher(IScabraObserverPublisher publisher)
        {
            _publisher = publisher;
        }

        public void StartPublishing()
        {
            _publishing = Task.Run(async () =>
            {
                while (!_isStopping)
                {
                    var topic = _rand.Next(10);
                    var text = _rand.Next(1000);
                    var msg = new SomeMessage(topic.ToString(), text.ToString(), BitConverter.GetBytes(topic));

                    _publisher.Publish(msg.Topic, msg);

                    await Task.Delay(_rand.Next(10));
                }
            });
        }

        public void StopPublishing()
        {
            _isStopping = true;

            if (!_publishing.Wait(1000))
                throw new ApplicationException();
        }
    }
}