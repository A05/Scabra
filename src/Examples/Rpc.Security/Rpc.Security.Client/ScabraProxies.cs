using Scabra.Rpc.Client;

namespace Scabra.Examples.Rpc.Security
{
    [RpcClientProxy]
    internal partial class SecurityServiceScabraProxy : ISecurityService
    {
    }

    [RpcClientProxy]
    internal partial class SomeProtectedServiceScabraProxy : ISomeProtectedService
    {
    }
}