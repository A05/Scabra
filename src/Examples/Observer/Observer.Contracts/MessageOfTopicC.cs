using ProtoBuf;

namespace Scabra.Examples.Observer
{
    [ProtoContract]
    public class MessageOfTopicC
    {
        [ProtoMember(1)]
        public string Payload { get; }

        private MessageOfTopicC() { }

        public MessageOfTopicC(string payload)
        {
            Payload = payload;
        }

        public override string ToString() => "C: " + Payload;
    }
}
