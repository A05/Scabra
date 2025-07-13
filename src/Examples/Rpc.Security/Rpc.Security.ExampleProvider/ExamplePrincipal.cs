using System;
using System.Security.Principal;

namespace Scabra.Examples.Rpc.Security
{
    public class ExamplePrincipal : IPrincipal
    {
        private readonly string[] _roles;

        public string AuthToken { get; }

        public IIdentity Identity { get; }

        public ExamplePrincipal(string authToken, string name, string[] roles)
        {
            AuthToken = authToken;
            Identity = new ExampleIdentity(name);
            _roles = roles;
        }
        
        public bool IsInRole(string role)
        {
            foreach (string r in _roles)
                if (r.Equals(role, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}
