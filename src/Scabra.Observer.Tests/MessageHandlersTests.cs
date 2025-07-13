using NUnit.Framework;
using Scabra.Observer.Subscriber;
using System;
using System.Linq;

namespace Scabra.Observer
{
    [TestFixture]
    public partial class MessageHandlersTests
    {
        [Test]
        public void should_crud_handler()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            var isFirstHandler = sut.Add<int>("topic", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);

            var handlers = sut.Get("topic");
            Assert.That(handlers, Is.Not.Null);
            Assert.That(handlers.Count(), Is.EqualTo(1));            
            Assert.That(handlers.First().messageType, Is.EqualTo(typeof(int)));

            handlers.First().handler(25);
            Assert.That(h._argInt, Is.EqualTo(25));

            var isLastHandler = sut.Remove<int>("topic", h.HandleInt);
            Assert.That(isLastHandler, Is.True);

            handlers = sut.Get("topic");
            Assert.That(handlers, Is.Not.Null);
            Assert.That(handlers.Count(), Is.EqualTo(0));
        }

        [Test]
        public void should_crud_different_handlers_with_same_topic()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            var isFirstHandler = sut.Add<int>("topic", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);
            
            isFirstHandler = sut.Add<string>("topic", h.HandleStr);
            Assert.That(isFirstHandler, Is.False);

            var handlers = sut.Get("topic");
            Assert.That(handlers, Is.Not.Null);
            Assert.That(handlers.Count(), Is.EqualTo(2));
            
            Assert.That(handlers.ElementAt(0).messageType, Is.EqualTo(typeof(int)));
            handlers.ElementAt(0).handler(25);
            Assert.That(h._argInt, Is.EqualTo(25));

            Assert.That(handlers.ElementAt(1).messageType, Is.EqualTo(typeof(string)));
            handlers.ElementAt(1).handler("48");
            Assert.That(h._argStr, Is.EqualTo("48"));

            var isLastHandler = sut.Remove<int>("topic", h.HandleInt);
            Assert.That(isLastHandler, Is.False);

            isLastHandler = sut.Remove<string>("topic", h.HandleStr);
            Assert.That(isLastHandler, Is.True);
        }

        [Test]
        public void should_crud_different_handlers_with_different_topics()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            var isFirstHandler = sut.Add<int>("topic1", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);

            isFirstHandler = sut.Add<string>("topic2", h.HandleStr);
            Assert.That(isFirstHandler, Is.True);

            var handlers1 = sut.Get("topic1");
            var handlers2 = sut.Get("topic2");

            Assert.That(handlers1, Is.Not.Null);
            Assert.That(handlers2, Is.Not.Null);
            Assert.That(handlers1.Count(), Is.EqualTo(1));
            Assert.That(handlers2.Count(), Is.EqualTo(1));
            
            Assert.That(handlers1.ElementAt(0).messageType, Is.EqualTo(typeof(int)));
            handlers1.ElementAt(0).handler(25);
            Assert.That(h._argInt, Is.EqualTo(25));

            Assert.That(handlers2.ElementAt(0).messageType, Is.EqualTo(typeof(string)));
            handlers2.ElementAt(0).handler("48");
            Assert.That(h._argStr, Is.EqualTo("48"));

            var isLastHandler = sut.Remove<int>("topic1", h.HandleInt);
            Assert.That(isLastHandler, Is.True);

            isLastHandler = sut.Remove<string>("topic2", h.HandleStr);
            Assert.That(isLastHandler, Is.True);
        }

        [Test]
        public void should_crud_same_handler_with_same_topic()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            var isFirstHandler = sut.Add<int>("topic", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);

            isFirstHandler = sut.Add<int>("topic", h.HandleInt);
            Assert.That(isFirstHandler, Is.False);

            var handlers = sut.Get("topic");

            Assert.That(handlers, Is.Not.Null);
            Assert.That(handlers.Count(), Is.EqualTo(2));
            
            Assert.That(handlers.ElementAt(0).messageType, Is.EqualTo(typeof(int)));
            handlers.ElementAt(0).handler(25);
            Assert.That(h._argInt, Is.EqualTo(25));
            
            Assert.That(handlers.ElementAt(1).messageType, Is.EqualTo(typeof(int)));
            handlers.ElementAt(1).handler(48);
            Assert.That(h._argInt, Is.EqualTo(48));

            var isLastHandler = sut.Remove<int>("topic", h.HandleInt);
            Assert.That(isLastHandler, Is.False);

            isLastHandler = sut.Remove<int>("topic", h.HandleInt);
            Assert.That(isLastHandler, Is.True);
        }

        [Test]
        public void should_crud_same_handler_with_different_topics()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            var isFirstHandler = sut.Add<int>("topic1", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);

            isFirstHandler = sut.Add<int>("topic2", h.HandleInt);
            Assert.That(isFirstHandler, Is.True);

            var handlers1 = sut.Get("topic1");
            var handlers2 = sut.Get("topic2");

            Assert.That(handlers1, Is.Not.Null);
            Assert.That(handlers2, Is.Not.Null);
            Assert.That(handlers1.Count(), Is.EqualTo(1));
            Assert.That(handlers2.Count(), Is.EqualTo(1));
            
            Assert.That(handlers1.ElementAt(0).messageType, Is.EqualTo(typeof(int)));
            handlers1.ElementAt(0).handler(25);
            Assert.That(h._argInt, Is.EqualTo(25));

            Assert.That(handlers2.ElementAt(0).messageType, Is.EqualTo(typeof(int)));
            handlers2.ElementAt(0).handler(48);
            Assert.That(h._argInt, Is.EqualTo(48));

            var isLastHandler = sut.Remove<int>("topic1", h.HandleInt);
            Assert.That(isLastHandler, Is.True);

            isLastHandler = sut.Remove<int>("topic2", h.HandleInt);
            Assert.That(isLastHandler, Is.True);
        }

        [Test]
        public void should_not_remove_not_existing_handler()
        {
            var sut = new MessageHandlers();
            var h = new HandlerHolder();

            Assert.Throws<InvalidOperationException>(() => sut.Remove<int>("topic", h.HandleInt));

            sut.Add<int>("topic", h.HandleInt);
            sut.Remove<int>("topic", h.HandleInt);

            Assert.Throws<InvalidOperationException>(() => sut.Remove<int>("topic", h.HandleInt));
            Assert.Throws<InvalidOperationException>(() => sut.Remove<string>("topic", h.HandleStr));
        }

        class HandlerHolder
        {
            public int _argInt;
            public string _argStr;

            public void HandleInt(int p) => _argInt = p;
            public void HandleStr(string s) => _argStr = s;
        }
    }
}
