using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra.Rpc.Client
{
    internal class ClientCall
    {
        private const int WaitingForReplySpinCount = 21;

        private readonly ManualResetEventSlim _mre;

        private ClientCallId _id;
        private int _timeoutInMs;
        private byte[] _callData;
        private byte[] _replyData;
        private bool _isAborted;

        public ClientCallId Id { get => _id; }
        public int TimeoutInMs { get => _timeoutInMs; }
        public byte[] CallData { get => _callData; }
        public byte[] ReplyData { get => _replyData; }
        public bool IsAborted { get => _isAborted; }

        public ClientCall(ushort index)
        {
            _id = new ClientCallId(index, 0);
            _mre = new(false, WaitingForReplySpinCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(ushort sequenceNumber, int timeoutInMs, byte[] callData)
        {
            Debug.Assert(callData != null, "Call data is null.");

            _id = new ClientCallId(_id.Index, sequenceNumber);
            _timeoutInMs = timeoutInMs;
            _callData = callData;
            _replyData = null;
            _isAborted = false;

            _mre.Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetReply(byte[] replyData)
        {
            Debug.Assert(replyData != null, "Reply data is null.");
            Debug.Assert(_replyData == null, "Reply data is already set.");

            // If the reply is aborted, then the call is already complete.

            if (!_isAborted)
            {
                _replyData = replyData;
                _mre.Set();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort()
        {
            // If the reply is set, then the call is already complete.

            if (_replyData == null)
            {
                _isAborted = true;
                _mre.Set();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait()
        {
            // TODO: (NU) This overhead should be measured and 
            // probably dynamically adjusted at runtime.
            const int ScabraRpcOverheadInMs = 100;

            return _mre.Wait(_timeoutInMs + ScabraRpcOverheadInMs);
        }

        public override string ToString() => _id + ", IsSet = " + _mre.IsSet;
    }
}
