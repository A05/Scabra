using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra.Rpc.Server
{
    internal class Call
    {
        private readonly List<byte[]> _callFrames = new(Reply.EnvelopCapacity + 1);
        private int _timeoutInMs;
        private CancellationTokenSource _cts;        

        public int TimeoutInMs { get => _timeoutInMs; }
        public CancellationToken CancellationToken { get => _cts.Token; }

        public IEnumerable<byte[]> ReplyEnvelop
        {
            get
            {
                for (int i = 0; i < _callFrames.Count - 1; i++)
                    yield return _callFrames[i];
            }
        }

        public byte[] CallData => _callFrames.Count > 0 ? _callFrames[_callFrames.Count - 1] : null;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IEnumerable<byte[]> callFrames, int timeoutInMs)
        {
            Debug.Assert(callFrames != null, "Call frames is null.");
            Debug.Assert(timeoutInMs > 0, "Timeout is invalid.");

            _callFrames.Clear();
            _callFrames.AddRange(callFrames);

            _timeoutInMs = timeoutInMs;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(Call call)
        {
            Debug.Assert(call != null);

            _callFrames.Clear();
            _callFrames.AddRange(call._callFrames);

            _timeoutInMs = call._timeoutInMs;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (call._cts.IsCancellationRequested)
                _cts.Cancel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort()
        {
            Debug.Assert(_cts.IsCancellationRequested == false, "Call is already aborted.");

            _cts.Cancel();
        }

        public override string ToString() =>
            $"Envelop (frames = {ReplyEnvelop.Count()}), " +
            $"Data (bytes = {CallData.Length}), " +
            $"Timeout = {_timeoutInMs} ms, " +
            $"Aborted = " + (_cts?.IsCancellationRequested ?? false);
    }
}
