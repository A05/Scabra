using System.Security.Principal;

namespace Scabra.Examples.Rpc.Security
{
    public class ExampleIdentity : IIdentity
    {
        public string Name { get; }

        public bool IsAuthenticated { get => true; }
        
        public string AuthenticationType { get => "ScabraSecurityExample"; }

        public ExampleIdentity(string name)
        {
            Name = name;
        }
    }
}
