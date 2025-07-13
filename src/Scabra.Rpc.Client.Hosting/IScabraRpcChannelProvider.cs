namespace Scabra.Rpc.Client.Hosting
{
    public interface IScabraRpcChannelProvider
    {
        IScabraRpcChannel GetChannel(string name);
    }
}
