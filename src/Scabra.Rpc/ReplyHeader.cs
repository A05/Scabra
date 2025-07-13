using System.Diagnostics;
using System.IO;

namespace Scabra.Rpc
{
    public class ReplyHeader
    {
        public static ReplyHeader Deserialize(Stream stream)
        {
            Debug.Assert(stream != null);

            var header = new ReplyHeader();

            var reader = new BinaryReader(stream);

            header.IsReplyNull = reader.ReadBoolean();
            header.ErrorCode = (RpcErrorCode) reader.ReadByte();
            header.ErrorDescription = reader.ReadString();

            return header;
        }

        public bool IsReplyNull { get; set; }

        public RpcErrorCode ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public void Serialize(Stream stream)
        {
            Debug.Assert(stream != null);

            var writer = new BinaryWriter(stream);

            writer.Write(IsReplyNull);
            writer.Write((byte)ErrorCode);
            writer.Write(ErrorDescription ?? string.Empty);
        }
    }
}
