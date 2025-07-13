using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scabra.Rpc.Client.Hosting
{
    public static class ScabraRpcClientServiceCollectionExtensions
    {
        private const string CONFIGURATION_SECTION = "scabra:rpc";

        public static IServiceCollection AddScabraRpcClient(
            this IServiceCollection services, 
            IConfigurationRoot configurationRoot)
        {
            return services.AddScabraRpcClient(configurationRoot, new NullScabraSecurityHandler());
        }

        public static IServiceCollection AddScabraRpcClient(
            this IServiceCollection services,
            IConfigurationRoot configurationRoot,
            IScabraSecurityHandler securityHandler)
        {
            if (securityHandler == null)
                throw new ArgumentNullException(nameof(securityHandler));

            var configuration = configurationRoot.GetSection(CONFIGURATION_SECTION);

            services
                .AddSingleton(securityHandler)
                .AddSingleton<IScabraRpcChannelProvider>(sp => {
                    return new ScabraRpcChannelProvider(configuration, securityHandler);
                });

            return services;
        }
    }
}
