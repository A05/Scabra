using ProtoBuf;

namespace Scabra.Examples.Docker
{
    [ProtoContract]
    public class SomeCall
    {
        [ProtoMember(1)]
        public int Delay { get; set; }

        [ProtoMember(2)]
        public byte[] ExtraField { get; set; }

        private SomeCall() { }

        public SomeCall(int delay)
        {
            Delay = delay;
        }
    }
}
