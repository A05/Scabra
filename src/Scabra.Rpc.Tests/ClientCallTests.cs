using NUnit.Framework;
using Scabra.Rpc.Client;

namespace Scabra.Rpc
{
    [TestFixture]
    public class ClientCallTests
    {
        private readonly byte[] _callData = new byte[] { 0, 1, 2, 3, 4, 5 };
        private readonly byte[] _replyData = new byte[] { 5, 4, 3, 2, 1, 0 };

        [Test]
        public void should_initialize_after_creation()
        {
            var sut = new ClientCall(index: 25);

            sut.Initialize(sequenceNumber: 48, timeoutInMs: 5, _callData);

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(48));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(5));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.Null);
            Assert.That(sut.IsAborted, Is.False);
            Assert.That(sut.Wait(), Is.False);
        }

        [Test]
        public void should_initialize_after_setting_reply()
        {
            var sut = new ClientCall(index: 25);

            sut.Initialize(sequenceNumber: 48, timeoutInMs: 5, _callData);
            sut.SetReply(_replyData);

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(48));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(5));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.EqualTo(_replyData));
            Assert.That(sut.IsAborted, Is.False);
            Assert.That(sut.Wait(), Is.True);

            sut.Initialize(sequenceNumber: 47, timeoutInMs: 4, _callData);

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(47));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(4));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.Null);
            Assert.That(sut.IsAborted, Is.False);
            Assert.That(sut.Wait(), Is.False);
        }

        [Test]
        public void should_initialize_after_aborting_by_dispose()
        {
            var sut = new ClientCall(index: 25);

            sut.Initialize(sequenceNumber: 48, timeoutInMs: 5, _callData);
            sut.Abort();

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(48));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(5));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.Null);
            Assert.That(sut.IsAborted, Is.True);
            Assert.That(sut.Wait(), Is.True);

            sut.Initialize(sequenceNumber: 47, timeoutInMs: 4, _callData);

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(47));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(4));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.Null);
            Assert.That(sut.IsAborted, Is.False);
            Assert.That(sut.Wait(), Is.False);
        }

        [Test]
        public void should_not_set_reply_after_aborting_by_dispose()
        {
            var sut = new ClientCall(index: 25);

            sut.Initialize(sequenceNumber: 48, timeoutInMs: 5, _callData);
            sut.Abort();
            sut.SetReply(_replyData);

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(48));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(5));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.Null);
            Assert.That(sut.IsAborted, Is.True);
            Assert.That(sut.Wait(), Is.True);
        }

        [Test]
        public void should_not_abort_by_dispose_after_setting_reply()
        {
            var sut = new ClientCall(index: 25);

            sut.Initialize(sequenceNumber: 48, timeoutInMs: 5, _callData);
            sut.SetReply(_replyData);
            sut.Abort();

            Assert.That(sut.Id.Index, Is.EqualTo(25));
            Assert.That(sut.Id.SequenceNumber, Is.EqualTo(48));
            Assert.That(sut.TimeoutInMs, Is.EqualTo(5));
            Assert.That(sut.CallData, Is.EqualTo(_callData));
            Assert.That(sut.ReplyData, Is.EqualTo(_replyData));
            Assert.That(sut.IsAborted, Is.False);
            Assert.That(sut.Wait(), Is.True);
        }
    }
}
