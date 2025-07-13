using ProtoBuf;

namespace Scabra.Examples.Docker
{
    [ProtoContract]
    public class SomeMessage 
    {
        [ProtoMember(1)]
        public string Topic { get; private set; }

        [ProtoMember(2)]
        public string Text { get; private set; }

        [ProtoMember(3)]
        public byte[] ExtraField { get; private set; }

        private SomeMessage() { }

        public SomeMessage(string topic, string text, byte[] extraField)
        {
            Topic = topic;
            Text = text;
            ExtraField = extraField;
        }
    }
}
