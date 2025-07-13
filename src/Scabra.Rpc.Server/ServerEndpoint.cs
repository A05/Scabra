using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc.Server
{
    internal class ServerEndpoint : Endpoint
    {
        private const int CallTimeoutInMs = 5000; // TODO: (U) Get from the call and set max allowed value.
        private const int RoutineCompletionTimeoutInMs = 500;
        private const int HandlersDisposingTimeoutInMs = 10_000; // TODO: (U) Get it from handlers. Use the max one.

        private readonly CallExecutor _callExecutor;
        private readonly int _handlersCount;
        private readonly CallHandler[] _handlers;
        private readonly List<byte[]> _callFrames;
        private readonly Reply _currentReply;

        private Task _endpointRoutine;

        internal ServerEndpoint(string address, CallExecutor callExecutor, ILogger logger) : base(address, logger)
        {
            _callExecutor = callExecutor;

            _handlersCount = (int) Math.Ceiling((double)Environment.ProcessorCount / 2);
            _handlers = new CallHandler[_handlersCount];

            _callFrames = new(Reply.EnvelopCapacity);
            _currentReply = new Reply();
        }

        public override void Start()
        {
            AssertNotStarted();

            for (var i = 0; i < _handlersCount; i++)
                _handlers[i] = new CallHandler(i, _callExecutor, this);

            _endpointRoutine = Task.Factory.StartNew(EndpointRoutine, TaskCreationOptions.LongRunning);

            EnsureStarted();
        }

        protected override void DisposeImpl()
        {
            var handlerDisposings = new Task[_handlers.Length];
            for (var i = 0; i < _handlers.Length; i++)
                handlerDisposings[i] = Task.Run(_handlers[i].Dispose); // TODO: (NU) Use pattern DisponseAsync

            if (!Task.WaitAll(handlerDisposings, HandlersDisposingTimeoutInMs))
                LogError("Call handlers disposing timeout: {Timeout} ms.", HandlersDisposingTimeoutInMs);

            CancellationTokenSource.Cancel();

            if (_endpointRoutine.Wait(RoutineCompletionTimeoutInMs))
                _endpointRoutine.Dispose();
            else
                LogError("Endpoint disposing timeout: {Timeout} ms.", RoutineCompletionTimeoutInMs);
        }

        private void EndpointRoutine()
        {
            try
            {
                using var socket = SocketFactory.CreateRouterSocket();
                socket.Bind(Address);

                State = EndpointState.Started;

                const int SleepIteration = 50_000;

                int rHandlerIdx = 0, sHandlerIdx = 0;

                for (int i = 0; !CancellationToken.IsCancellationRequested; i = i == SleepIteration ? 0 : i + 1)
                {
                    var receivedAny = RecieveCalls(socket, rHandlerIdx);

                    var sentAny = false;
                    for (int j = (sHandlerIdx + 1) % _handlers.Length; true; j = (j + 1) % _handlers.Length)
                    {
                        if (!_handlers[j].Replies.IsEmpty)
                        {
                            sHandlerIdx = j;
                            sentAny = SendReplies(socket, sHandlerIdx);
                            break;
                        }

                        if (j == sHandlerIdx)
                            break;
                    }

                    if (receivedAny || sentAny)
                        i = 0;

                    Thread.Sleep(i == SleepIteration ? 1 : 0);

                    rHandlerIdx = (rHandlerIdx + 1) % _handlers.Length;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!CancellationToken.IsCancellationRequested)
                {
                    State = EndpointState.Failed;
                    LogError("Endpoint crashed.", ex);

                    OnCrashed(new EndpointCrashedEventArgs(ex));
                }
            }
        }

        private bool RecieveCalls(NetMqRouterSocket socket, int handlerIdx)
        {
            bool receivedAny = false;

            while (socket.HasIn)
            {
                if (!receivedAny) _callFrames.Clear();

                receivedAny = true;

                var frame = socket.Receive(out bool hasMore);

                if (State == EndpointState.Started) 
                    _callFrames.Add(frame);

                if (!hasMore)
                {
                    if (State == EndpointState.Started)
                    {
                        var calls = _handlers[handlerIdx].Calls;
                        calls.Enqueue(_callFrames, CallTimeoutInMs);
                    }
                    else
                        LogWarning("Call rejected. Endpoint disposing.");
                }
            }

            return receivedAny;
        }
        
        private bool SendReplies(NetMqRouterSocket socket, int handlerIdx)
        {
            bool sentAny = false;

            var replies = _handlers[handlerIdx].Replies;
            if (!replies.IsEmpty && replies.TryDequeue(_currentReply))
            {
                sentAny = true;

                Debug.Assert(_currentReply.Data != null);

                foreach (var frame in _currentReply.Envelop)
                    socket.SendMore(frame);

                socket.Send(_currentReply.Data);
            }

            return sentAny;
        }
    }
}
