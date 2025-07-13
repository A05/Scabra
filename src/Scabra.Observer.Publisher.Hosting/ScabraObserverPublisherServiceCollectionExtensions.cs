using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scabra.Observer.Publisher.Hosting
{
    public static class ScabraObserverPublisherServiceCollectionExtensions
    {
        private const string CONFIGURATION_SECTION = "scabra:observer:publisher";

        public static IServiceCollection AddScabraObserverPublisher(
            this IServiceCollection services,
            IConfigurationRoot configurationRoot)
        {
            var configuration = configurationRoot.GetSection(CONFIGURATION_SECTION);
            var publisherOptions = configuration.Get<PublisherOptions>();

            services
                .AddSingleton(publisherOptions)
                .AddSingleton<IScabraObserverPublisher, ScabraObserverPublisher>()
                .AddHostedService<ScabraObserverPublisherHostedService>();

            return services;
        }
    }
}
