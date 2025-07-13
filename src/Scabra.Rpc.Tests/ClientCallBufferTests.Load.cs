using NUnit.Framework;
using Scabra.Rpc.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    public partial class ClientCallBufferTests
    {
        const int NumberOfTasks = 8;
        const int NumberOfIterations = 10_000;

        /////////////////////////////////////////////////
        //
        // Adjust only this number for your environment.
        // The less this number, the better performance.
        //
        /////////////////////////////////////////////////
        const ushort IterationDurationInMs = 60;

        readonly bool DebugOutputEnabled = false;

        [Test]
        [Category("Load")]
        public void should_handle_high_load()
        {
            var sut = new ClientCallBuffer(_logger);
            var tasksStarted = new CountdownEvent(NumberOfTasks);
            var lastIterationsStarted = new CountdownEvent(NumberOfTasks);

            var tasks = new Task[NumberOfTasks];
            for (byte i = 0; i < tasks.Length; i++)
            {
                var taskNumber = i; // avoid closure.
                tasks[i] = Task.Run(() => NormalFlow(taskNumber, sut, tasksStarted, lastIterationsStarted));
            }

            if (!tasksStarted.Wait(millisecondsTimeout: NumberOfTasks * 100))
                Assert.Fail("Timeout of starting the tasks.");

            if (!lastIterationsStarted.Wait(NumberOfIterations * IterationDurationInMs))
                Assert.Fail("Timeout of waiting last iterations started.");

            var waited = sut.WaitForCompletion(timeoutInMs: NumberOfTasks * 300);
            Assert.That(waited, Is.True, "Timeout of waiting for completion.");

            for (int i = 0; i < tasks.Length; i++) 
            {
                Assert.That(tasks[i].IsCompleted, Is.True);

                var message = tasks[i].IsFaulted ? tasks[i].Exception.ToString() : "success";
                Assert.That(tasks[i].IsFaulted, Is.False, message);
            }
        }

        private void NormalFlow(byte taskNumber, ClientCallBuffer sut, CountdownEvent taskStarted, CountdownEvent lastIterationsStarted) 
        {
            const int SmallTimeoutInMs = 1, AverageTimeoutInMs = 2, LargeTimeoutInMs = 3;

            Exception firstException = null;
            bool lisSignaled = false;

            taskStarted.Signal();
            
            if (DebugOutputEnabled)
                Console.WriteLine($"Task {taskNumber} started.");

            for (int j = 1; firstException == null && j <= NumberOfIterations; j++)
            {
                ClientCall executingCall = null;

                try
                {
                    var callData = new byte[] { taskNumber, (byte)(j >> 24 & 0xFF), (byte)(j >> 16 & 0xFF), (byte)(j >> 8 & 0xFF), (byte)(j & 0xFF) };
                    var replyData = new byte[] { (byte)(j >> 24 & 0xFF), (byte)(j >> 16 & 0xFF), (byte)(j >> 8 & 0xFF), (byte)(j & 0xFF), taskNumber };

                    var timeout = j >= 1 && j <= 3 ? SmallTimeoutInMs : (j % 2 == 0 ? AverageTimeoutInMs : LargeTimeoutInMs);

                    var call = sut.AddPending(callData, timeout);
                    if (call.CallData != callData)
                        throw new Exception("Invalid call data on adding pending.");

                    if (j == NumberOfIterations)
                    {
                        lisSignaled = true;
                        lastIterationsStarted.Signal();
                    }

                    if (!sut.TryGetPendingForExecuting(out executingCall))
                        throw new Exception("TryGetPendingForExecuting: false");

                    if (executingCall.TimeoutInMs == LargeTimeoutInMs)
                    {
                        if (!sut.TrySetReply(executingCall.Id, replyData))
                            throw new Exception("TrySetReply: false");
                    }
                    else if (executingCall.TimeoutInMs == AverageTimeoutInMs)
                        executingCall.Abort();

                    var isCallTimeouted = !executingCall.Wait();

                    if (executingCall.TimeoutInMs == LargeTimeoutInMs && isCallTimeouted)
                        throw new Exception("Setting reply for a call did not complete that call.");
                    else if (executingCall.TimeoutInMs == AverageTimeoutInMs && isCallTimeouted)
                        throw new Exception("Aborting a call did not complete that call.");
                    else if (executingCall.TimeoutInMs == SmallTimeoutInMs && !isCallTimeouted)
                        throw new Exception("A call completed without setting reply or aborting.");

                    if (executingCall.CallData == null)
                        throw new Exception("Invalid call data on an executing call.");

                    if (executingCall.TimeoutInMs == LargeTimeoutInMs)
                    {
                        if (executingCall.ReplyData == null)
                            throw new Exception("Invalid reply data.");
                        else if (DebugOutputEnabled)
                        {
                            var iteration = executingCall.ReplyData[0] << 24 | executingCall.ReplyData[1] << 16 | executingCall.ReplyData[2] << 8 | executingCall.ReplyData[3];
                            Console.WriteLine(
                                $"Task {taskNumber}, iteration {j} receive reply: " +
                                $"task {executingCall.ReplyData[2]}, iteration {iteration}.");
                        }
                    }
                    else if (executingCall.TimeoutInMs == AverageTimeoutInMs && !executingCall.IsAborted)
                        throw new Exception("Aborting did not mark a call as aborted.");
                }
                catch (Exception ex)
                {
                    firstException = ex;
                    if (DebugOutputEnabled)
                        Console.WriteLine($"Task {taskNumber} failed. {ex}");
                }
                finally
                {
                    try
                    {
                        if (executingCall != null)
                            sut.Remove(executingCall);
                    }
                    catch (Exception ex)
                    {
                        firstException ??= ex;
                        if (DebugOutputEnabled)
                            Console.WriteLine($"Task {taskNumber} failed. {ex}");
                    }
                }
            }

            if (firstException != null)
            {
                if (!lisSignaled)
                    lastIterationsStarted.Signal();

                throw firstException;
            }

            if (DebugOutputEnabled)
                Console.WriteLine($"Task {taskNumber} completed.");
        }

        [Test]
        [Category("Load")]
        public void should_abort_high_load()
        {
            var sut = new ClientCallBuffer(_logger);
            var tasksStarted = new CountdownEvent(NumberOfTasks);

            var tasks = new Task[NumberOfTasks];
            for (byte i = 0; i < tasks.Length; i++)
            {
                var taskNumber = i; // avoid closure.
                tasks[i] = Task.Run(() => AbortFlow(taskNumber, sut, tasksStarted));
            }

            if (!tasksStarted.Wait(millisecondsTimeout: NumberOfTasks * 100))
                Assert.Fail("Timeout of starting the tasks.");

            const int timeoutBeforeAborting = 50;
            var waited = sut.WaitForCompletion(timeoutInMs: NumberOfTasks * timeoutBeforeAborting);
            Assert.That(waited, Is.False, "Calls completed without aborting.");

            sut.Abort();

            const int timeoutAfterAborting = timeoutBeforeAborting / 2;
            waited = sut.WaitForCompletion(timeoutInMs: NumberOfTasks * timeoutAfterAborting);
            Assert.That(waited, Is.True, "Timeout of waiting for completion after aborting.");

            for (int i = 0; i < tasks.Length; i++)
            {
                Assert.That(tasks[i].IsCompleted, Is.True);

                var message = tasks[i].IsFaulted ? tasks[i].Exception.ToString() : "success";
                Assert.That(tasks[i].IsFaulted, Is.False, message);
            }
        }

        private void AbortFlow(byte taskNumber, ClientCallBuffer sut, CountdownEvent taskStarted)
        {
            Exception firstException = null;

            taskStarted.Signal();

            if (DebugOutputEnabled)
                Console.WriteLine($"Task {taskNumber} started.");

            ClientCall call = null;

            try
            {
                call = sut.AddPending(_callData, timeoutInMs: 10_000);

                if (taskNumber % 2 == 0)
                    sut.TryGetPendingForExecuting(out _);

                call.Wait();

                if (!call.IsAborted)
                    throw new Exception("Call must be aborted in this test.");
            }
            catch (Exception ex)
            {
                firstException = ex;
                if (DebugOutputEnabled)
                    Console.WriteLine($"Task {taskNumber} failed. {ex}");
            }
            finally
            {
                try
                {
                    if (call != null)
                        sut.Remove(call);
                }
                catch (Exception ex)
                {
                    firstException ??= ex;
                    if (DebugOutputEnabled)
                        Console.WriteLine($"Task {taskNumber} failed. {ex}");
                }
            }

            if (firstException != null)
                throw firstException;

            if (DebugOutputEnabled)
                Console.WriteLine($"Task {taskNumber} completed.");
        }
    }
}
