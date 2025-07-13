using System;
using System.IO;

namespace Scabra
{
    public class ProtoBufPayloadSerializer : IPayloadSerializer
    {   
        void IPayloadSerializer.Serialize(Stream stream, object obj)
        {
            ProtoBuf.Serializer.NonGeneric.Serialize(stream, obj);
        }

        object IPayloadSerializer.Deserialize(Type type, Stream stream)
        {
            Type nxType;

            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                nxType = typeof(Nullable<>).MakeGenericType(type);
            else
                nxType = type;
            
            var obj = ProtoBuf.Serializer.NonGeneric.Deserialize(nxType, stream);

            if (obj == null && nxType != type)
                throw new ScabraException("A null value cannot be deserialized to a non-nullable value type.");
            
            return obj;
        }
    }
}
