using ProtoBuf;

namespace Scabra.Benchmarking.Rpc
{
    [ProtoContract]
    public class ComplexEntity
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int[] Marks { get; set; }

        [ProtoMember(4)]
        public string Description { get; set; }

        public ComplexEntity() { }
    }
}
