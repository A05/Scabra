using System;
using System.IO;

namespace Scabra
{
    public interface IPayloadSerializer
    {
        void Serialize(Stream stream, object obj);

        object Deserialize(Type type, Stream stream);
    }
}
