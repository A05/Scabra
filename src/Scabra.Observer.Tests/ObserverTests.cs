using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scabra.Observer.Publisher;
using Scabra.Observer.Subscriber;
using System;
using System.Threading;

namespace Scabra.Observer
{
    [TestFixture]
    public partial class ObserverTests
    {
        private IScabraObserverPublisher _publisher;
        private IScabraObserverSubscriber _subscriber;

        [SetUp]
        public void SetUp()
        {
            var publisherOptions = new PublisherOptions()
            {
                Address = "inproc://observer-test-server"
            };

            _publisher = new ScabraObserverPublisher(publisherOptions, NullLoggerFactory.Instance);
            _publisher.Start();

            var subscriberOptions = new ScabraObserverSubscriberOptions()
            {
                Address = "inproc://observer-test-server"
            };

            _subscriber = new ScabraObserverSubscriber(subscriberOptions, NullLoggerFactory.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            _publisher?.Dispose();
            _subscriber?.Dispose();
        }

        [Test]
        public void should_receive_message_for_any_topic()
        {
            int receivedId = 0;

            Action<ObserverMessage> handler = m => receivedId = m.Id;
            _subscriber.Subscribe(topic: "", handler);

            _publisher.Publish("DNM", new ObserverMessage(1));
            AssertThatChanged(() => receivedId == 1, timeoutInMs: 50, "Message should be handled.");

            _publisher.Publish("", new ObserverMessage(25));
            AssertThatChanged(() => receivedId == 25, timeoutInMs: 50, "Message should be handled.");
            
            _subscriber.Unsubscribe(topic: "", handler);

            _publisher.Publish("", new ObserverMessage(48));
            AssertThatNotChanged(() => receivedId == 25, periodInMs: 50, "Message should not be handled.");
        }

        [Test]
        public void should_receive_message_for_specified_topic()
        {
            int receivedId = 0;

            Action<ObserverMessage> handler = m => receivedId = m.Id;
            _subscriber.Subscribe(topic: "my-topic", handler);

            _publisher.Publish("another", new ObserverMessage(1));
            AssertThatNotChanged(() => receivedId == 0, periodInMs: 50, "Message should not be handled.");

            _publisher.Publish("my-topic", new ObserverMessage(25));
            AssertThatChanged(() => receivedId == 25, timeoutInMs: 50, "Message should be handled.");

            receivedId = 0;

            _publisher.Publish("", new ObserverMessage(48));
            AssertThatNotChanged(() => receivedId == 0, periodInMs: 50, "Message should not be handled.");

            _subscriber.Unsubscribe(topic: "my-topic", handler);

            _publisher.Publish("", new ObserverMessage(193));
            AssertThatNotChanged(() => receivedId == 0, periodInMs: 50, "Message should not be handled.");

            _publisher.Publish("my-topic", new ObserverMessage(3));
            AssertThatNotChanged(() => receivedId == 0, periodInMs: 50, "Message should not be handled.");
        }

        private void AssertThatChanged(Func<bool> predicate, int timeoutInMs, string failMessage)
        {
            var start = Environment.TickCount;

            while (!predicate())
            {
                if (Environment.TickCount - start >= timeoutInMs)
                    Assert.Fail(failMessage);

                Thread.Sleep(1);
            }
        }

        private void AssertThatNotChanged(Func<bool> predicate, int periodInMs, string failMessage)
        {
            var start = Environment.TickCount;

            while (predicate())
            {
                if (Environment.TickCount - start >= periodInMs)
                    return;

                Thread.Sleep(1);
            }

            Assert.Fail(failMessage);
        }
    }
}
