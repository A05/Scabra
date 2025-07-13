using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Scabra.Examples.Rpc.Security
{
    internal class SomeClient
    {
        private readonly ISecurityService _securityService;
        private readonly ISomeProtectedService _protectedService;
        private readonly ILogger<SomeClient> _logger;

        public SomeClient(ISecurityService securityService, ISomeProtectedService protectedService, ILogger<SomeClient> logger)
        {
            _protectedService = protectedService ?? throw new ArgumentNullException(nameof(protectedService));            
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void DemonstrateSecurityUsage()
        {
            DoUnauthenticatedRpc();

            Login("invalid", "invalid");

            DoUnauthenticatedRpc();

            Login("example", "12345");

            _protectedService.DoProtectedJob(); // Authenticated call.
            _logger.LogInformation("Protected job is done.");

            Logout();            

            DoUnauthenticatedRpc();
        }

        private void Login(string name, string password)
        {
            var authToken = _securityService.GetAuthToken(name, password);
            _logger.LogInformation("Login: {Success}.", authToken != null);

            if (authToken != null)
                Thread.CurrentPrincipal = ExampleScabraSecurityHandler.GetPrincipal(authToken);
        }

        private void Logout()
        {
            Thread.CurrentPrincipal = null;

            _logger.LogInformation("Logout.");
        }

        private void DoUnauthenticatedRpc()
        {
            try
            {
                _protectedService.DoProtectedJob(); // Not authenticated call.
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to do the protected job. {Errors}", toErrors(ex));
            }

            string toErrors(Exception ex) => ex != null ? ex.Message + toErrors(ex.InnerException) : string.Empty;
        }
    }
}
