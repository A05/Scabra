using System;
using System.Security;
using System.Threading;

namespace Scabra.Examples.Rpc.Security
{
    public class SomeProtectedService : ISomeProtectedService
    {
        public void DoProtectedJob()
        {
            var principal = Thread.CurrentPrincipal;

            if (principal?.Identity?.IsAuthenticated != true)
                throw new SecurityException("Call is not authenticated.");

            if (principal.Identity.Name != "example")
                throw new UnauthorizedAccessException("Access denied.");

            if (!principal.IsInRole("role1") || !principal.IsInRole("role2") || !principal.IsInRole("role3"))
                throw new UnauthorizedAccessException("Access denied.");
        }
    }
}