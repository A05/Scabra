using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Scabra.Rpc.Server
{
    internal class Reply
    {
        public const int EnvelopCapacity = 8;

        private readonly List<byte[]> _envelop = new(EnvelopCapacity);
        private byte[] _data = Array.Empty<byte>();

        public byte[] Data { get => _data; }
        public IEnumerable<byte[]> Envelop => _envelop;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IEnumerable<byte[]> envelop, byte[] data)
        {
            Debug.Assert(envelop != null, "Reply envelop is null.");
            Debug.Assert(data != null, "Reply data is null.");

            _envelop.Clear();
            _envelop.AddRange(envelop);

            _data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(Reply reply)
        {
            Debug.Assert(reply != null);

            _envelop.Clear();
            _envelop.AddRange(reply._envelop);

            _data = reply._data;
        }

        public override string ToString() => $"Envelop (frames = {_envelop.Count}), Data (bytes = {_data.Length})";
    }
}
