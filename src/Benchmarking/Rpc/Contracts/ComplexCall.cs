using System.Runtime.Serialization;

namespace Scabra.Benchmarking.Rpc
{
    [DataContract]
    public class ComplexCall
    {
        [DataMember(Order = 1)]
        public ComplexEntity Parameter1 { get; set; }

        [DataMember(Order = 2)]
        public ComplexEntity Parameter2 { get; set; }

        [DataMember(Order = 3)]
        public ComplexEntity Parameter3 { get; set; }
    }
}
