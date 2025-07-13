using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scabra.Rpc.Server.Hosting
{
    public static class ScabraRpcServerServiceCollectionExtensions
    {
        private const string CONFIGURATION_SECTION = "scabra:rpc:server";

        public static IServiceCollection AddScabraRpcServer(
            this IServiceCollection services, 
            IConfigurationRoot configurationRoot)
        {
            return services.AddScabraRpcServer(configurationRoot, new NullScabraSecurityHandler());
        }

        public static IServiceCollection AddScabraRpcServer(
            this IServiceCollection services,
            IConfigurationRoot configurationRoot,
            IScabraSecurityHandler securityHandler)
        {
            if (securityHandler == null)
                throw new ArgumentNullException(nameof(securityHandler));

            var configuration = configurationRoot.GetSection(CONFIGURATION_SECTION);
            var serverOptions = configuration.Get<ScabraRpcServerOptions>();

            services
                .AddSingleton(serverOptions)
                .AddSingleton<IScabraRpcServer, ScabraRpcServer>()
                .AddSingleton(securityHandler)
                .AddHostedService<ScabraRpcServerHostedService>();

            return services;
        }
    }
}
