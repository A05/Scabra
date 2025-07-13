using System;
using Scabra.Rpc;

namespace Scabra.Examples.Rpc.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly ExampleScabraSecurityHandler _securityHandler;

        public SecurityService(IScabraSecurityHandler securityHandler) 
        {
            if (securityHandler is not ExampleScabraSecurityHandler)
                throw new NotSupportedException();

            _securityHandler = (ExampleScabraSecurityHandler) securityHandler;
        }

        public string GetAuthToken(string name, string password)
        {
            if (name != "example" || password != "12345")
                return null;
            
            return _securityHandler.CreateAuthToken(name, "role1", "role2", "role3");
        }
    }
}