using System.Runtime.Serialization;

namespace Scabra.Benchmarking.Rpc
{
    [DataContract]
    public class PrimitiveCall
    {
        [DataMember(Order = 1)]
        public int Parameter1 { get; set; }

        [DataMember(Order = 2)]
        public int Parameter2 { get; set; }

        [DataMember(Order = 3)]
        public int Parameter3 { get; set; }
    }
}
