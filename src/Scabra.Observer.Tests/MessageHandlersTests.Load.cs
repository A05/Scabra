using NUnit.Framework;
using Scabra.Observer.Subscriber;
using System;
using System.Threading.Tasks;

namespace Scabra.Observer
{
    public partial class MessageHandlersTests
    {
        const int NumberOfTasks = 8;
        const int NumberOfIterations = 1000;

        /////////////////////////////////////////////////
        //
        // Adjust only this number for your environment.
        // The less this number, the better performance.
        //
        /////////////////////////////////////////////////
        const ushort IterationDurationInMs = 60;

        [Test]
        [Category("Load")]
        public void should_handle_high_load()
        {
            var sut = new MessageHandlers();

            var tasks = new Task[NumberOfTasks];
            for (byte i = 0; i < tasks.Length; i++)
            {
                var taskNumber = i; // avoid closure.
                tasks[i] = Task.Run(() => NormalFlow(taskNumber, sut));
            }

            if (!Task.WaitAll(tasks, millisecondsTimeout: NumberOfIterations * IterationDurationInMs))
                Assert.Fail("Tasks timeout.");

            for (int i = 0; i < tasks.Length; i++)
            {
                Assert.That(tasks[i].IsCompleted, Is.True);

                var message = tasks[i].IsFaulted ? tasks[i].Exception.ToString() : "success";
                Assert.That(tasks[i].IsFaulted, Is.False, message);
            }
        }

        private void NormalFlow(byte taskNumber, MessageHandlers sut)
        {
            Exception firstException = null;

            var id = Guid.NewGuid();
            var h = new LoadHandlerHolder(id);

            for (int j = 1; firstException == null && j <= NumberOfIterations; j++)
            {
                try
                {
                    var topic = j % 2 == 0 ? "topic 1" : "topic 2";

                    sut.Add<Guid>(topic, h.Handle);

                    var handlers = sut.Get(topic);

                    foreach (var (messageType, handler) in handlers)
                        handler(id);

                    if (!h._called) 
                        throw new Exception($"Handler is not added: task = {taskNumber}, j = {j}.");

                    h._called = false;

                    sut.Remove<Guid>(topic, h.Handle);

                    handlers = sut.Get(topic);

                    foreach (var (messageType, handler) in handlers)
                        handler(id);

                    if (h._called)
                        throw new Exception($"Handler is not removed: task = {taskNumber}, j = {j}.");
                }
                catch (Exception ex)
                {
                    firstException = ex;
                }
            }

            if (firstException != null)
                throw firstException;
        }

        class LoadHandlerHolder
        {
            private readonly Guid _id;
            public bool _called;

            public LoadHandlerHolder(Guid id) => _id = id;

            public void Handle(Guid id)
            {
                if (_id == id)
                    _called = true;
            }
        }
    }
}
