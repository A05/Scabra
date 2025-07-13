using System;
using System.Threading.Tasks;
using Scabra.Observer.Publisher;

namespace Scabra.Examples.Observer
{
    public class SomeService : IDisposable
    {
        private readonly Task _publishing;
        private bool _disposed;

        public SomeService(IScabraObserverPublisher publisher) 
        {
            _publishing = Task.Run(async () => 
            {
                for (int i = 0; !_disposed; i++) 
                {
                    var message_a = new MessageOfTopicA($"message # {i}");
                    publisher.Publish("topic_a", message_a);
                    Console.WriteLine($"{message_a} published.");

                    var message_b = new MessageOfTopicB($"message # {i}");
                    publisher.Publish("topic_b", message_b);
                    Console.WriteLine($"{message_b} published.");

                    var message_c = new MessageOfTopicC($"message # {i}");
                    publisher.Publish(i % 2 == 0 ? "topic_a" : "topic_b", message_c);
                    Console.WriteLine($"{message_c} published.");

                    await Task.Delay(500);
                }
            });
        }

        public void Dispose()
        {
            _disposed = true;

            _publishing.Wait(1000);
        }
    }
}