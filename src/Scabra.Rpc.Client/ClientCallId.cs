using System;

namespace Scabra.Rpc.Client
{
    internal readonly struct ClientCallId : IEquatable<ClientCallId>
    {
        public readonly ushort Index;
        public readonly ushort SequenceNumber;

        public ClientCallId(ushort index, ushort sequenceNumber) 
        { 
            Index = index; SequenceNumber = sequenceNumber;
        }

        public ClientCallId(byte[] indexBytes, byte[] sequenceNumberBytes)
        {
            Index = BitConverter.ToUInt16(indexBytes, 0);
            SequenceNumber = BitConverter.ToUInt16(sequenceNumberBytes, 0);
        }

        public (byte[] indexBytes, byte[] sequenceNumberBytes) ToBytes()
        {
            var indexBytes = BitConverter.GetBytes(Index);
            var sequenceNumberBytes = BitConverter.GetBytes(SequenceNumber);
                        
            return (indexBytes, sequenceNumberBytes);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientCallId id && Equals(id);
        }

        public bool Equals(ClientCallId other)
        {
            return Index == other.Index && SequenceNumber == other.SequenceNumber;
        }

        public override int GetHashCode()
        {
            int hashCode = -1197030641;
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + SequenceNumber.GetHashCode();
            return hashCode;
        }

        public override string ToString() => "Index = " + Index + ", SequenceNumber = " + SequenceNumber;

        public static bool operator ==(ClientCallId left, ClientCallId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ClientCallId left, ClientCallId right)
        {
            return !(left == right);
        }
    }
}
