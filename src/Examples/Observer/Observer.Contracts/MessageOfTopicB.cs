using ProtoBuf;

namespace Scabra.Examples.Observer
{
    [ProtoContract]
    public class MessageOfTopicB 
    {
        [ProtoMember(1)]
        public string Payload { get; }

        private MessageOfTopicB() { }

        public MessageOfTopicB(string payload)
        {
            Payload = payload;
        }

        public override string ToString() => "B: " + Payload;
    }
}
