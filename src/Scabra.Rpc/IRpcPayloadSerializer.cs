using System;
using System.IO;

namespace Scabra.Rpc
{
    public interface IRpcPayloadSerializer : IPayloadSerializer
    {
        void SerializeArgs(Stream stream, object[] objs);

        object[] DeserializeArgs(Stream stream, Type[] types, ushort argsIndices);
    }
}
