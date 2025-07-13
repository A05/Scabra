using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Observer.Subscriber
{
    internal class SubscribeEndpoint : Endpoint
    {
        class Message
        {
            public string Topic { get; }
            public byte[] Data { get; }

            public Message(string topic, byte[] data)
            {
                Topic = topic;
                Data = data;
            }
        }

        class Command
        {
            public bool ShouldSubscribe { get; }
            public string Topic { get; }

            public Command(bool shouldSubscribe, string topic)
            {
                ShouldSubscribe = shouldSubscribe;
                Topic = topic;
            }
        }

        private const int QueuesAreEmptyTimeoutInMs = 10_000; // TODO: (NU) Do it smartly.
        private const int RoutineCompletionTimeoutInMs = 500;

        private readonly ConcurrentQueue<Command> _commands = new();
        private readonly ConcurrentQueue<Message> _messages = new();

        private readonly Task _endpointRoutine;
        private readonly Task _eventRaiserRoutine;

        internal event Action<string, byte[]> MessageReceived;

        public SubscribeEndpoint(string address, ILogger logger) : base (address, logger)
        {
            _endpointRoutine = Task.Factory.StartNew(EndpointRoutine, TaskCreationOptions.LongRunning);            
            _eventRaiserRoutine = Task.Factory.StartNew(EventRaiserRoutine, TaskCreationOptions.LongRunning);

            EnsureStarted();
        }

        protected override void DisposeImpl()
        {
            if (!WaitForQueuesAreEmpty(QueuesAreEmptyTimeoutInMs))
                LogError("{Timeout} ms timeout on waiting for queues are empty.", QueuesAreEmptyTimeoutInMs);

            CancellationTokenSource.Cancel();

            if (_endpointRoutine.Wait(RoutineCompletionTimeoutInMs))
                _endpointRoutine.Dispose();
            else
                LogError("{Timeout} ms timeout on waiting for endpoint routine to complete.", RoutineCompletionTimeoutInMs);

            if (_eventRaiserRoutine.Wait(RoutineCompletionTimeoutInMs))
                _eventRaiserRoutine.Dispose();
            else
                LogError("{Timeout} ms timeout on waiting for event raiser routine to complete.", RoutineCompletionTimeoutInMs);
        }

        public void Subscribe(string topic)
        {
            if (State == EndpointState.Disposing || State == EndpointState.Disposed)
                throw new ObjectDisposedException(nameof(SubscribeEndpoint));
            
            _commands.Enqueue(new Command(shouldSubscribe: true, topic));
        }

        public void Unsubscribe(string topic)
        {
            if (State == EndpointState.Disposing || State == EndpointState.Disposed)
                throw new ObjectDisposedException(nameof(SubscribeEndpoint));

            _commands.Enqueue(new Command(shouldSubscribe: false, topic));
        }

        private void EndpointRoutine()
        {
            State = EndpointState.Started;

            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    using var socket = SocketFactory.CreateSubscriberSocket();
                    socket.Connect(Address);

                    while (!CancellationToken.IsCancellationRequested)
                    {
                        while (_commands.TryDequeue(out Command c))
                        {
                            if (c.ShouldSubscribe)
                                socket.Subscribe(c.Topic);
                            else
                                socket.Unsubscribe(c.Topic);
                        }

                        while (socket.HasIn)
                        {
                            var topic = socket.ReceiveString(out bool hasMore);
                            if (hasMore)
                            {
                                var data = socket.Receive(out hasMore);
                                if (!hasMore)
                                    _messages.Enqueue(new Message(topic, data));
                                else
                                {
                                    LogError("'{Topic}' topic. Invalid message format: more than one data frames.", topic);

                                    while (hasMore)
                                        socket.Receive(out hasMore);
                                }
                            }
                            else
                                LogError("'{Topic}' topic. Invalid message format: no data.", topic);
                        }

                        Thread.Sleep(0);
                    }
                }
            }
            catch (Exception ex)
            {
                State = EndpointState.Failed;
                LogError("Endpoint routine crashed.", ex);
            }
        }

        private void EventRaiserRoutine()
        {
            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    while (_messages.TryDequeue(out Message m))
                    {
                        try
                        {
                            MessageReceived?.Invoke(m.Topic, m.Data);
                        }
                        catch (Exception ex)
                        {
                            LogError("Failed to raise a message received event.", ex);
                        }
                    }

                    Thread.Sleep(0);
                }
            }
            catch (Exception ex)
            {
                State = EndpointState.Failed;
                LogError("Event raiser routine crashed.", ex);
            }
        }

        private bool WaitForQueuesAreEmpty(int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (_commands.Count > 0 || _messages.Count > 0)
            {
                if (Environment.TickCount - start >= timeoutInMs)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }
    }
}
