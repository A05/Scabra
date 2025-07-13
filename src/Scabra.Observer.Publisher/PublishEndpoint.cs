using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Observer.Publisher
{
    internal class PublishEndpoint : Endpoint
    {
        class Message
        {
            public string Topic { get;}
            public byte[] Data { get; }

            public Message(string topic, byte[] data)
            {
                Topic = topic;
                Data = data;
            }
        }

        private const int QueueIsEmptyTimeoutInMs = 10_000; // TODO: (NU) Do it smartly.
        private const int RoutineCompletionTimeoutInMs = 500;

        private readonly bool _shouldConnect;
        private readonly ConcurrentQueue<Message> _toBePublished = new();
        private Task _endpointRoutine;       

        public PublishEndpoint(string address, ILogger logger) : base(address, logger)
        {
            // ScabraOptions.SETTINGS_CONNECTION_TYPE && ScabraOptions.SETTINGS_CONNECTION_TYPE_CONNECT
            // _shouldConnect must be set to TRUE when this endpoint is used in XPub/XSub pattern.
            //
            _shouldConnect = false;
        }

        public override void Start()
        {
            _endpointRoutine = Task.Factory.StartNew(EndpointRoutine, TaskCreationOptions.LongRunning);

            EnsureStarted();
        }

        protected override void DisposeImpl()
        {
            if (!WaitForQueueIsEmpty(QueueIsEmptyTimeoutInMs))
                LogError("{Timeout} ms timeout on waiting for queue is empty.", QueueIsEmptyTimeoutInMs);

            CancellationTokenSource.Cancel();

            if (_endpointRoutine.Wait(RoutineCompletionTimeoutInMs))
                _endpointRoutine.Dispose();
            else
                LogError("Endpoint disposing timeout: {Timeout} ms.", RoutineCompletionTimeoutInMs);
        }

        public void Publish(string topic, byte[] data)
        {
            if (State == EndpointState.Disposing || State == EndpointState.Disposed)
                throw new ObjectDisposedException(nameof(PublishEndpoint));

            _toBePublished.Enqueue(new Message(topic, data));
        }

        private void EndpointRoutine()
        {
            try
            {
                using var socket = SocketFactory.CreatePublisherSocket();

                if (_shouldConnect)
                    socket.Connect(Address);
                else
                    socket.Bind(Address);

                State = EndpointState.Started;

                const int SleepIteration = 50_000;

                for (int i = 0; !CancellationToken.IsCancellationRequested; i = i == SleepIteration ? 0 : i + 1)
                {
                    while (_toBePublished.TryDequeue(out Message m))
                    {
                        socket.SendMore(m.Topic);
                        socket.Send(m.Data);

                        i = 0;
                    }

                    Thread.Sleep(i == SleepIteration ? 1 : 0);
                }
            }
            catch (Exception ex)
            {
                if (!CancellationToken.IsCancellationRequested)
                {
                    State = EndpointState.Failed;
                    LogError("Endpoint crashed.", ex);
                }
            }
        }

        private bool WaitForQueueIsEmpty(int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (_toBePublished.Count > 0)
            {
                if (Environment.TickCount - start >= timeoutInMs)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }
    }
}
