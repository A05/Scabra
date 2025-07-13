using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scabra.Observer.Subscriber.Hosting
{
    public static class ScabraObserverSubscriberServiceCollectionExtensions
    {
        private const string CONFIGURATION_SECTION = "scabra:observer";

        public static IServiceCollection AddScabraObserverSubscriber(
            this IServiceCollection services,
            IConfigurationRoot configurationRoot)
        {
            var configuration = configurationRoot.GetSection(CONFIGURATION_SECTION);

            services
                .AddSingleton<IScabraObserverSubscriberProvider>(sp => {
                    return new ScabraObserverSubscriberProvider(configuration);
                });

            return services;
        }
    }
}
