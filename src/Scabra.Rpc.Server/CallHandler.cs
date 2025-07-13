using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc.Server
{
    internal sealed class CallHandler : IDisposable
    {        
        private const int RoutineCompletionTimeoutInMs = 1100; // TODO: (U) Synch with call timeout.
        private const int CallsProcessingTimeoutInMs = 10_000; // TODO: (U) Calculate it from CallTimeoutInMs-s.
        private const int AbortedCallsProcessingTimeoutInMs = 1000;
        private const int RepliesProcessingTimeoutInMs = 10_000;

        private readonly int _id;
        private readonly Task _routine;
        private readonly CallQueue _calls;
        private readonly Call _currentCall;
        private readonly ReplyQueue _replies;        
        private readonly CallExecutor _callExecutor;        
        private readonly CancellationTokenSource _cts;
        private readonly IEndpointLogger _logger;

        public CallQueue Calls => _calls;
        public ReplyQueue Replies => _replies;

        public CallHandler(int id, CallExecutor callExecutor, IEndpointLogger logger)
        {
            _id = id;
            _callExecutor = callExecutor;
            _calls = new CallQueue(logger);
            _currentCall = new Call();
            _replies = new ReplyQueue(logger);
            _logger = logger;
            
            _cts = new CancellationTokenSource();

            _routine = Task.Factory.StartNew(Routine, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            if (!_calls.WaitUntilEmpty(CallsProcessingTimeoutInMs))
            {
                _logger.LogError("Calls processing timeout: {Timeout} ms.", CallsProcessingTimeoutInMs);

                _calls.Abort();

                if (!_calls.WaitUntilEmpty(AbortedCallsProcessingTimeoutInMs))
                    _logger.LogError("Aborted calls processing timeout: {Timeout} ms.", AbortedCallsProcessingTimeoutInMs);
            }

            if (!_replies.WaitUntilEmpty(RepliesProcessingTimeoutInMs))
                _logger.LogError("Replies processing timeout: {Timeout} ms.", RepliesProcessingTimeoutInMs);

            _cts.Cancel();

            if (_routine.Wait(RoutineCompletionTimeoutInMs))
                _routine.Dispose();
            else
                _logger.LogError("Call handler {HandlerId} disposing timeout: {Timeout} ms.", _id, RoutineCompletionTimeoutInMs);
        }

        private void Routine()
        {
            try
            {
                const int SleepIteration = 10_000;

                for (int i = 0; !_cts.IsCancellationRequested; i = i == SleepIteration ? 0 : i + 1)
                {
                    if (_calls.Wait(100) && _calls.TryDequeue(_currentCall))
                    {
                        var replyData = _callExecutor.Execute(_currentCall);
                        Debug.Assert(replyData != null);

                        _replies.Enqueue(_currentCall.ReplyEnvelop, replyData);

                        i = 0;
                    }

                    Thread.Sleep(i == SleepIteration ? 1 : 0);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Call handler {Id} crashed.", _id);
            }            
        }

        public override string ToString() => $"Id = {_id}, Calls = ({_calls}), Replies = ({_replies}), IsCancellationRequested = {_cts.IsCancellationRequested}";
    }
}
