using System;
using System.Diagnostics;
using System.IO;

namespace Scabra.Rpc
{
    public class RpcProtoBufPayloadSerializer : ProtoBufPayloadSerializer, IRpcPayloadSerializer
    {
        private readonly int _maxArgsLength;

        public RpcProtoBufPayloadSerializer(int maxArgsLength) 
        {
            _maxArgsLength = maxArgsLength;
        }

        void IRpcPayloadSerializer.SerializeArgs(Stream stream, object[] objs)
        {
            for (var i = 0; i < objs.Length; i++)
                if (objs[i] != null)
                    ProtoBuf.Serializer.NonGeneric.SerializeWithLengthPrefix(stream, objs[i], ProtoBuf.PrefixStyle.Base128, fieldNumber: i + 1);
        }

        object[] IRpcPayloadSerializer.DeserializeArgs(Stream stream, Type[] types, ushort argsIndices)
        {
            Debug.Assert(types.Length <= _maxArgsLength);

            var objs = new object[types.Length];
            if (objs.Length > 0)
            {
                var typeResolver = new ProtoBuf.Serializer.TypeResolver(i => types[i - 1]);

                for (ushort i = 0, r = 1; i < objs.Length; i++, r <<= 1)
                {
                    if ((argsIndices & r) != 0)
                    {
                        if (!ProtoBuf.Serializer.NonGeneric.TryDeserializeWithLengthPrefix(stream, ProtoBuf.PrefixStyle.Base128, typeResolver, out object obj))
                            throw new Exception($"Failed to deserialize an object into {typeResolver(i).FullName} type.");

                        objs[i] = obj;
                    }
                }
            }

            return objs;
        }
    }
}
