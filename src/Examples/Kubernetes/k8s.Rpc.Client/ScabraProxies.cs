using Scabra.Rpc.Client;

namespace Scabra.Examples.k8s.Rpc
{
    [RpcClientProxy]
    internal partial class PingPongServiceScabraProxy : IPingPongService
    {
    }
}