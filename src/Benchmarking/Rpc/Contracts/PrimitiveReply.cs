using System.Runtime.Serialization;

namespace Scabra.Benchmarking.Rpc
{
    [DataContract]
    public class PrimitiveReply
    {
        [DataMember(Order = 1)]
        public int Value { get; set; }
    }
}
