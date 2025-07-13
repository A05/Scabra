using ProtoBuf;

namespace Scabra.Observer
{
    public partial class ObserverTests
    {
        [ProtoContract]
        public class ObserverMessage
        {
            [ProtoMember(1)]
            public int Id { get; private set; }

            public ObserverMessage() { }

            public ObserverMessage(int id) => Id = id;
        }
    }
}
