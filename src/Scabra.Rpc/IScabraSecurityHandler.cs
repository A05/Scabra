namespace Scabra.Rpc
{
    public interface IScabraSecurityHandler
    {
        object DecodeSecret(byte[] bytes);

        byte[] EncodeSecret(object secret);

        // TODO: (NU) it must return something covertable to PermissionDenied || Unauthenticated
        // which allows to return the correct RpcErrorCode.
        void TakeSecurityMeasures(object secret);

        object GetSecret();
    }
}
