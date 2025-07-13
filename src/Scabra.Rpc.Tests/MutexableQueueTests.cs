using Moq;
using NUnit.Framework;
using Scabra.Rpc.Server;
using System;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    [TestFixture]
    public class MutexableQueueTests
    {
        [Test]
        public void should_enqueue_dequeue()
        {
            var sut = new TestMutexableQueue(10);
            Assert.That(sut.IsEmpty, Is.True);

            sut.Enqueue(25);
            Assert.That(sut.IsEmpty, Is.False);

            TestItem item = new();
            var success = sut.TryDequeue(item);
            Assert.That(success, Is.True);
            Assert.That(sut.IsEmpty, Is.True);
            Assert.That(item.P, Is.EqualTo(25));
            Assert.That(item.State, Is.Null);
        }

        [Test]
        public void should_not_enqueue_if_queue_is_full()
        {
            var sut = new TestMutexableQueue(1);
            sut.Enqueue(1);

            Assert.Throws<InvalidOperationException>(() => sut.Enqueue(2));
        }

        [Test]
        public void should_dequeue_in_fifo()
        {
            var sut = new TestMutexableQueue(10);
            
            sut.Enqueue(1);
            sut.Enqueue(2);
            sut.Enqueue(3);

            TestItem item = new();
            sut.TryDequeue(item);
            Assert.That(item.P, Is.EqualTo(1));

            sut.TryDequeue(item);
            Assert.That(item.P, Is.EqualTo(2));

            sut.TryDequeue(item);
            Assert.That(item.P, Is.EqualTo(3));
        }

        [Test]
        public void should_not_dequeue_if_queue_is_empty()
        {
            var sut = new TestMutexableQueue(10);

            TestItem item = new();
            var success = sut.TryDequeue(item);
            Assert.That(success, Is.False);
        }

        [Test]
        public void should_wait_until_empty()
        {
            var sut = new TestMutexableQueue(10);

            sut.Enqueue(1);
            sut.Enqueue(2);
            sut.Enqueue(3);

            var waiting = Task.Run(() => sut.WaitUntilEmpty(timeoutInMs: 10));

            TestItem item = new();

            sut.TryDequeue(item);
            sut.TryDequeue(item);
            sut.TryDequeue(item);

            var success = waiting.Wait(30);
            Assert.That(success, Is.True);
            Assert.That(waiting.Result, Is.True);
        }

        [Test]
        public void should_time_out_on_waiting_until_empty()
        {
            var sut = new TestMutexableQueue(10);

            sut.Enqueue(1);

            var waiting = Task.Run(() => sut.WaitUntilEmpty(timeoutInMs: 10));

            var success = waiting.Wait(30);
            Assert.That(success, Is.True);
            Assert.That(waiting.Result, Is.False);
        }

        [Test]
        public void should_release_item_on_dequeue()
        {
            var sut = new TestMutexableQueue(10);

            TestItem item_0 = new();
            sut.Enqueue(0);
            sut.TryDequeue(item_0);
            item_0.State = new object();

            TestItem item_1 = new();
            sut.Enqueue(1);
            sut.TryDequeue(item_1);
            
            Assert.That(item_0.State, Is.Not.Null);
            Assert.That(item_1.State, Is.Null);
        }

        private class TestItem
        {
            public int? P = null;
            public object State = new();

            public void Initialize(int p)
            {
                P = p;
                State = null;
            }

            public void CopyFrom(TestItem item)
            {
                P = item.P;
                State = item.State;
            }
        }

        private class TestMutexableQueue : MutexableQueue<TestItem>
        {
            public TestMutexableQueue(int length) : base(length, initialTimeoutInMs: 1, gainAccessTimeoutInMs: 10, Mock.Of<IEndpointLogger>())
            {
            }

            public void Enqueue(int p)
            {
                Enqueue(item => item.Initialize(p));
            }

            public bool TryDequeue(TestItem item)
            {
                return TryDequeue(item.CopyFrom);
            }
        }
    }
}
