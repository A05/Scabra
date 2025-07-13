using ProtoBuf;

namespace Scabra.Examples.Observer
{
    [ProtoContract]
    public class MessageOfTopicA 
    {
        [ProtoMember(1)]
        public string Payload { get; }

        private MessageOfTopicA() { }

        public MessageOfTopicA(string payload)
        {
            Payload = payload;
        }

        public override string ToString() => "A: " + Payload;
    }
}
