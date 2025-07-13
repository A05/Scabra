using Moq;
using NUnit.Framework;
using Scabra.Rpc.Server;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    [TestFixture]
    public class CallQueueTests
    {
        [Test]
        public void should_wait_for_enqueue()
        {
            var sut = new CallQueue(Mock.Of<IEndpointLogger>());

            var waiting = Task.Run(() => sut.Wait(10));

            sut.Enqueue(new[] { new byte[] { 1, 2, 3 } }, 1);

            var sucess = waiting.Wait(30);
            Assert.That(sucess, Is.True);
            Assert.That(waiting.Result, Is.True);
        }

        [Test]
        public void should_time_out_on_waiting_for_enqueue()
        {
            var sut = new CallQueue(Mock.Of<IEndpointLogger>());

            var waiting = Task.Run(() => sut.Wait(10));

            var sucess = waiting.Wait(30);
            Assert.That(sucess, Is.True);
            Assert.That(waiting.Result, Is.False);
        }

        [Test]
        public void should_abort_calls()
        {
            var sut = new CallQueue(Mock.Of<IEndpointLogger>());

            sut.Enqueue(new[] { new byte[] { 1, 2, 3 } }, 1);
            sut.Enqueue(new[] { new byte[] { 1, 2, 3 } }, 1);
            sut.Enqueue(new[] { new byte[] { 1, 2, 3 } }, 1);

            Call call = new();
            sut.TryDequeue(call);
            Assert.That(call.CancellationToken.IsCancellationRequested, Is.False);

            sut.Abort();
                        
            sut.TryDequeue(call);
            Assert.That(call.CancellationToken.IsCancellationRequested, Is.True);
            sut.TryDequeue(call);
            Assert.That(call.CancellationToken.IsCancellationRequested, Is.True);
        }
    }
}
