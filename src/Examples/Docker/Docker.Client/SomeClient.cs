using System;
using System.Threading;
using System.Threading.Tasks;
using Scabra.Observer.Subscriber;
using Scabra.Rpc.Client;

namespace Scabra.Examples.Docker
{
    internal class SomeClient
    {
        private readonly IScabraObserverSubscriber _subscriber;
        private readonly ISomeService _someService;

        private Task _rpcCycle, _observerCycle;
        private CancellationTokenSource _cts = new();

        public SomeClient(IScabraRpcChannel rpcChannel, IScabraObserverSubscriber subscriber) 
        {
            _subscriber = subscriber;

            _someService = new SomeServiceScabraProxy(rpcChannel);
        }

        public void StartRemoteJob()
        {
            int rpcOkCount = 0, rpcFailCount = 0, observerOkCount = 0, observerFailCount = 0;

            _rpcCycle = Task.Run(async () =>
            {
                var rand = new Random((int)DateTime.Now.Ticks);

                int? i1, i2, @case = 0;
                TimeSpan? ts1, ts2;
                bool? b1, b2;

                while (!_cts.IsCancellationRequested)
                {
                    if (@case == 0)
                    {
                        i1 = 1; i2 = null;
                        ts1 = TimeSpan.FromSeconds(2); ts2 = null;
                        b1 = true; b2 = null;
                        @case++;
                    }
                    else if (@case == 1)
                    {
                        i1 = 1; i2 = 2;
                        ts1 = TimeSpan.FromSeconds(3); ts2 = TimeSpan.FromSeconds(4);
                        b1 = false; b2 = true;
                        @case++;
                    }
                    else
                    {
                        i1 = i2 = null;
                        ts1 = ts2 = null;
                        b1 = b2 = null;
                        @case = 0;
                    }

                    var expectedMessage = $"i1={i1}, i2={i2}, ts1={ts1}, ts2={ts2}, b1={b1}, b2={b2}";

                    var actualMessage = _someService.AcceptValues(i1, i2, ts1, ts2, b1, b2);
                        
                    if (actualMessage == expectedMessage)
                        rpcOkCount++;
                    else
                        rpcFailCount++;

                    try
                    {
                        var delay = rand.Next(1000);
                        var call = new SomeCall(delay) 
                        {
                            ExtraField = BitConverter.GetBytes(delay)
                        };

                        var actualDelay = _someService.ExecuteWithDelay(call);
                        if (actualDelay == call.Delay)
                            rpcOkCount++;
                        else
                            rpcFailCount++;
                    }
                    catch (Exception)
                    {
                        rpcFailCount++;
                    }

                    writeLog();

                    await Task.Delay(10);
                }
            });

            _rpcCycle.ContinueWith(t => { Console.WriteLine(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);

            _observerCycle = Task.Run(async () =>
            {
                Action<SomeMessage> handler = (SomeMessage message) =>
                {
                    if (!int.TryParse(message.Topic, out var topicAsInt))
                        observerFailCount++;
                    else
                    {
                        var expExtraField = BitConverter.GetBytes(topicAsInt);
                        if (equals(expExtraField, message.ExtraField))
                            observerOkCount++;
                        else
                            observerFailCount++;
                    }

                    writeLog();
                };

                _subscriber.Subscribe("", handler);

                while (!_cts.IsCancellationRequested)
                    await Task.Delay(10);

                _subscriber.Unsubscribe("", handler);
            });

            _observerCycle.ContinueWith(t => { Console.WriteLine(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);

            Console.WriteLine("Client started.");

            void writeLog() => Console.WriteLine($"RR-ok = {rpcOkCount}/RR-failed = {rpcFailCount}\t PS-ok = {observerOkCount}/PS-failed = {observerFailCount}");

            bool equals(byte[] e, byte[] a)
            {
                if (e.Length != a.Length)
                    return false;

                for (int i = 0; i < a.Length; i++)
                    if (e[i] != a[i])
                        return false;

                return true;
            }
        }

        public void StopRemoteJob()
        {
            // TODO: There is a bug in the stop routine which blocks the current call forever.
            // So, the cancellation does not work.

            _cts.Cancel(); 

            if (!Task.WaitAll(new[] { _rpcCycle, _observerCycle }, 5000))
                throw new ApplicationException();
        }
    }
}