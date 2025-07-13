using System.Diagnostics;
using System.IO;

namespace Scabra.Rpc
{
    public class CallHeader
    {
        public static CallHeader Deserialize(Stream stream)
        {
            Debug.Assert(stream != null);

            var header = new CallHeader();

            var reader = new BinaryReader(stream);

            header.Service = reader.ReadString();
            header.Method = reader.ReadString();
            header.ArgsIndices = reader.ReadUInt16();

            var secretLength = reader.ReadInt32();
            if (secretLength != 0)
                header.Secret = reader.ReadBytes(secretLength);

            return header;
        }

        public string Service { get; set; }

        public string Method { get; set; }

        public ushort ArgsIndices { get; set; }

        public byte[] Secret { get; set; }

        public void Serialize(Stream stream)
        {
            Debug.Assert(stream != null);

            var writer = new BinaryWriter(stream);

            Debug.Assert(Service != null);
            writer.Write(Service);

            Debug.Assert(Method != null);
            writer.Write(Method);

            writer.Write(ArgsIndices);

            if (Secret == null)
                writer.Write(0);
            else
            {
                writer.Write(Secret.Length);
                writer.Write(Secret);
            }
        }

        public override string ToString() => $"{Service}.{Method}";
    }
}
