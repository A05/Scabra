using Moq;
using NUnit.Framework;
using Scabra.Rpc.Client;
using System;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    [TestFixture]
    public partial class ClientCallBufferTests
    {
        private const int DoesNotMatter = 1;
        private const int GainAccessTimeoutInMs = 10;

        private readonly byte[] _callData = new byte[] { 0, 1, 2, 3, 4, 5 };
        private readonly byte[] _replyData = new byte[] { 5, 4, 3, 2, 1, 0 };
        private readonly IEndpointLogger _logger = Mock.Of<IEndpointLogger>();

        [Test]
        public void should_add_pending()
        {
            var sut = new ClientCallBuffer(_logger);

            var call_1 = sut.AddPending(_callData, timeoutInMs: 123);

            Assert.That(call_1, Is.Not.Null);
            Assert.That(call_1.TimeoutInMs, Is.EqualTo(123));
            Assert.That(call_1.CallData, Is.EqualTo(_callData));
            Assert.That(call_1.ReplyData, Is.Null);
        }

        [Test]
        public void should_not_add_pending_if_buffer_is_full()
        {
            var sut = new ClientCallBuffer(length: 1, GainAccessTimeoutInMs, _logger);

            sut.AddPending(_callData, timeoutInMs: DoesNotMatter);

            // Now buffer is full as length is 1.

            Assert.Throws<InvalidOperationException>(() => sut.AddPending(_callData, timeoutInMs: DoesNotMatter));
        }

        [Test]
        public void should_initialize_call_on_add_pending()
        {
            var sut = new ClientCallBuffer(length: 1, GainAccessTimeoutInMs, _logger);

            var originalCall = sut.AddPending(_callData, 123);
            
            // We test that reply data is cleaned up on the second AddPending.
            originalCall.SetReply(_replyData);
            Assert.IsNotNull(originalCall.ReplyData);

            // Copy call ID with original sequence number.
            var originalCallId = originalCall.Id;

            sut.Remove(originalCall); // Return the original call to the free list.

            var newCall = sut.AddPending(_callData, 123);
            
            Assert.That(ReferenceEquals(originalCall, newCall), Is.True);
            Assert.That(originalCallId.Index, Is.EqualTo(newCall.Id.Index));
            Assert.That(originalCallId.SequenceNumber, Is.Not.EqualTo(newCall.Id.SequenceNumber));

            Assert.IsNull(newCall.ReplyData);
        }

        [Test]
        public void should_not_remove_free_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            sut.Remove(call);

            // Here we are trying to remove the call from free list.
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call));
        }

        [Test]
        public void should_remove_pending_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            
            sut.Remove(call);
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call));
        }

        [Test]
        public void should_remove_executing_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            sut.TryGetPendingForExecuting(out _);

            sut.Remove(call);
            Assert.Throws<InvalidOperationException>(() => sut.Remove(call));
        }

        [Test]
        public void should_not_remove_not_existing_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var notExistingCall = new ClientCall(1);
            notExistingCall.Initialize(234, 567, _callData);

            Assert.Throws<InvalidOperationException>(() => sut.Remove(notExistingCall));
        }

        [Test]
        public void should_not_remove_equals_by_value_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call = sut.AddPending(_callData, 123);

            var valueEqualCall = new ClientCall(call.Id.Index);
            valueEqualCall.Initialize(call.Id.SequenceNumber, call.TimeoutInMs, call.CallData);

            Assert.Throws<InvalidOperationException>(() => sut.Remove(valueEqualCall));
        }

        [Test]
        public void should_get_pending_for_executing_by_fifo()
        {
            var sut = new ClientCallBuffer(_logger);

            var call_1 = sut.AddPending(_callData, 123);
            var call_2 = sut.AddPending(_callData, 123);
            var call_3 = sut.AddPending(_callData, 123);

            var success = sut.TryGetPendingForExecuting(out var call);
            Assert.That(success, Is.True);
            Assert.That(ReferenceEquals(call_1, call), Is.True);

            success = sut.TryGetPendingForExecuting(out call);
            Assert.That(success, Is.True);
            Assert.That(ReferenceEquals(call_2, call), Is.True);

            success = sut.TryGetPendingForExecuting(out call);
            Assert.That(success, Is.True);
            Assert.That(ReferenceEquals(call_3, call), Is.True);
        }

        [Test]
        public void should_not_get_pending_for_executing_for_free_calls()
        {
            var sut = new ClientCallBuffer(_logger);

            var success = sut.TryGetPendingForExecuting(out var _);

            Assert.That(success, Is.False);
        }

        [Test]
        public void should_not_get_pending_for_executing_for_executing_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            sut.AddPending(_callData, 123);
            sut.TryGetPendingForExecuting(out _);

            var success = sut.TryGetPendingForExecuting(out var _);

            Assert.That(success, Is.False);
        }

        [Test]
        public void should_not_set_reply_for_absent_call()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);

            var success = sut.TrySetReply(new ClientCallId(0, 48), _replyData);
            Assert.That(success, Is.False);

            success = sut.TrySetReply(new ClientCallId(25, 48), _replyData);
            Assert.That(success, Is.False);
        }

        [Test]
        public void should_not_set_reply_for_free_call()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);
            var call = sut.AddPending(_callData, timeoutInMs: 123);

            sut.Remove(call); // After removing the call becomes free.

            var success = sut.TrySetReply(call.Id, _replyData);
            
            Assert.That(success, Is.False);
            Assert.That(call.ReplyData, Is.Null);
        }

        [Test]
        public void should_not_set_reply_for_pending_call()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);
            var call = sut.AddPending(_callData, timeoutInMs: 123);

            var success = sut.TrySetReply(call.Id, _replyData);
            
            Assert.That(success, Is.False);
            Assert.That(call.ReplyData, Is.Null);
        }

        [Test]
        public void should_set_reply_for_executing_call()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);
            var call = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            sut.TryGetPendingForExecuting(out var _);

            var success = sut.TrySetReply(call.Id, _replyData);

            Assert.That(success, Is.True);
            Assert.That(call.ReplyData, Is.EqualTo(_replyData));
        }

        [Test]
        public void should_wait_for_pending_calls_completion()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);
            var call = sut.AddPending(_callData, timeoutInMs: 1);

            var waitingForCompletion = Task.Run(() => sut.WaitForCompletion(timeoutInMs: 5));
            var waited = waitingForCompletion.Wait(100);
            Assert.That(waited, Is.True);
            Assert.That(waitingForCompletion.Result, Is.False);

            waitingForCompletion = Task.Run(() => sut.WaitForCompletion(timeoutInMs: 10));
            
            sut.Remove(call);

            waited = waitingForCompletion.Wait(100);
            Assert.That(waited, Is.True);
            Assert.That(waitingForCompletion.Result, Is.True);
        }

        [Test]
        public void should_wait_for_executing_calls_completion()
        {
            var sut = new ClientCallBuffer(1, GainAccessTimeoutInMs, _logger);
            var call = sut.AddPending(_callData, timeoutInMs: 1);
            sut.TryGetPendingForExecuting(out var _);

            var waitingForCompletion = Task.Run(() => sut.WaitForCompletion(timeoutInMs: 5));
            var waited = waitingForCompletion.Wait(100);
            Assert.That(waited, Is.True);
            Assert.That(waitingForCompletion.Result, Is.False);

            waitingForCompletion = Task.Run(() => sut.WaitForCompletion(timeoutInMs: 10));

            sut.Remove(call);

            waited = waitingForCompletion.Wait(100);
            Assert.That(waited, Is.True);
            Assert.That(waitingForCompletion.Result, Is.True);
        }

        [Test]
        public void should_wait_for_free_calls_completion()
        {
            var sut = new ClientCallBuffer(_logger);

            var waitingForCompletion = Task.Run(() => sut.WaitForCompletion(timeoutInMs: 5));
            var waited = waitingForCompletion.Wait(100);

            Assert.That(waited, Is.True);
            Assert.That(waitingForCompletion.Result, Is.True);
        }

        [Test]
        public void should_abort_pending_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call_1 = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            var call_2 = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);

            sut.Abort();

            Assert.That(call_1.IsAborted, Is.True);
            Assert.That(call_2.IsAborted, Is.True);
        }

        [Test]
        public void should_abort_executing_calls()
        {
            var sut = new ClientCallBuffer(_logger);
            var call_1 = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            var call_2 = sut.AddPending(_callData, timeoutInMs: DoesNotMatter);
            sut.TryGetPendingForExecuting(out var _);
            sut.TryGetPendingForExecuting(out var _);

            sut.Abort();

            Assert.That(call_1.IsAborted, Is.True);
            Assert.That(call_2.IsAborted, Is.True);
        }
    }
}
