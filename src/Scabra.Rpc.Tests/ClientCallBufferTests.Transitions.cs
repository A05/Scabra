using NUnit.Framework;
using Scabra.Rpc.Client;
using System;
using System.Linq;

namespace Scabra.Rpc
{
    public partial class ClientCallBufferTests
    {
#if DEBUG
        [Test]
        public void should_transit_between_states()
        {
            var sut = new ClientCallBuffer(8, gainAccessTimeoutInMs: 10, _logger);

            assertFree("0, 1, 2, 3, 4, 5, 6, 7");
            assertPending("");
            assertExecuting("");

            var call_0 = sut.AddPending(_callData, 1);
            var call_1 = sut.AddPending(_callData, 1);
            var call_2 = sut.AddPending(_callData, 1);

            assertFree("3, 4, 5, 6, 7");
            assertPending("0, 1, 2");
            assertExecuting("");

            Assert.That(call_0.Id.SequenceNumber, Is.EqualTo(0));
            Assert.That(call_1.Id.SequenceNumber, Is.EqualTo(1));
            Assert.That(call_2.Id.SequenceNumber, Is.EqualTo(2));

            sut.TryGetPendingForExecuting(out _);
            
            assertFree("3, 4, 5, 6, 7");
            assertPending("1, 2");
            assertExecuting("0");

            sut.TrySetReply(call_0.Id, _replyData);

            assertFree("3, 4, 5, 6, 7");
            assertPending("1, 2");
            assertExecuting("0");

            sut.Remove(call_0);

            assertFree("3, 4, 5, 6, 7, 0");
            assertPending("1, 2");
            assertExecuting("");

            var call_3 = sut.AddPending(_callData, 1);
            var call_4 = sut.AddPending(_callData, 1);

            assertFree("5, 6, 7, 0");
            assertPending("1, 2, 3, 4");
            assertExecuting("");

            Assert.That(call_1.Id.SequenceNumber, Is.EqualTo(1));
            Assert.That(call_2.Id.SequenceNumber, Is.EqualTo(2));
            Assert.That(call_3.Id.SequenceNumber, Is.EqualTo(3));
            Assert.That(call_4.Id.SequenceNumber, Is.EqualTo(4));

            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);

            assertFree("5, 6, 7, 0");
            assertPending("");
            assertExecuting("1, 2, 3, 4");

            sut.Abort();

            assertFree("5, 6, 7, 0");
            assertPending("");
            assertExecuting("1, 2, 3, 4");

            sut.Remove(call_2);

            assertFree("5, 6, 7, 0, 2");
            assertPending("");
            assertExecuting("1, 3, 4");

            sut.Remove(call_1);

            assertFree("5, 6, 7, 0, 2, 1");
            assertPending("");
            assertExecuting("3, 4");

            sut.Remove(call_3);

            assertFree("5, 6, 7, 0, 2, 1, 3");
            assertPending("");
            assertExecuting("4");

            var call_5 = sut.AddPending(_callData, 1);
            var call_6 = sut.AddPending(_callData, 1);
            var call_7 = sut.AddPending(_callData, 1);
            sut.AddPending(_callData, 1);
            sut.AddPending(_callData, 1);
            sut.AddPending(_callData, 1);
            sut.AddPending(_callData, 1);            

            assertFree("");
            assertPending("5, 6, 7, 0, 2, 1, 3");
            assertExecuting("4");

            Assert.That(call_4.Id.SequenceNumber, Is.EqualTo(4));
            Assert.That(call_5.Id.SequenceNumber, Is.EqualTo(5));
            Assert.That(call_6.Id.SequenceNumber, Is.EqualTo(6));
            Assert.That(call_7.Id.SequenceNumber, Is.EqualTo(7));
            Assert.That(call_0.Id.SequenceNumber, Is.EqualTo(8));
            Assert.That(call_2.Id.SequenceNumber, Is.EqualTo(9));
            Assert.That(call_1.Id.SequenceNumber, Is.EqualTo(10));
            Assert.That(call_3.Id.SequenceNumber, Is.EqualTo(11));

            Assert.Throws<InvalidOperationException>(() => sut.AddPending(_callData, 1));

            assertFree("");
            assertPending("5, 6, 7, 0, 2, 1, 3");
            assertExecuting("4");

            sut.Remove(call_5);

            assertFree("5");
            assertPending("6, 7, 0, 2, 1, 3");
            assertExecuting("4");

            sut.Remove(call_7);

            assertFree("5, 7");
            assertPending("6, 0, 2, 1, 3");
            assertExecuting("4");

            sut.Remove(call_3);

            assertFree("5, 7, 3");
            assertPending("6, 0, 2, 1");
            assertExecuting("4");

            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);
            sut.TryGetPendingForExecuting(out _);

            assertFree("5, 7, 3");
            assertPending("");
            assertExecuting("4, 6, 0, 2, 1");

            Assert.That(sut.TryGetPendingForExecuting(out _), Is.False);

            assertFree("5, 7, 3");
            assertPending("");
            assertExecuting("4, 6, 0, 2, 1");

            sut.Remove(call_0);
            sut.Remove(call_4);
            sut.Remove(call_1);
            sut.Remove(call_6);
            sut.Remove(call_2);

            assertFree("5, 7, 3, 0, 4, 1, 6, 2");
            assertPending("");
            assertExecuting("");

            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_0));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_1));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_2));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_3));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_4));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_5));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_6));
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call_7));

            void assertFree(string expected)
            {
                var actual = string.Join(", ", sut.FreeCalls.Select(i => i.ToString()));
                Assert.That(actual, Is.EqualTo(expected));
            }

            void assertPending(string expected)
            {
                var actual = string.Join(", ", sut.PendingCalls.Select(i => i.ToString()));
                Assert.That(actual, Is.EqualTo(expected));
            }

            void assertExecuting(string expected)
            {
                var actual = string.Join(", ", sut.ExecutingCalls.Select(i => i.ToString()));
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
#endif
}
