namespace Scabra.Rpc
{
    public class NullScabraSecurityHandler : IScabraSecurityHandler
    {
        public object DecodeSecret(byte[] bytes) => null;

        public byte[] EncodeSecret(object secret) => null;

        public void TakeSecurityMeasures(object secret) {}

        public object GetSecret() => null;
    }
}
