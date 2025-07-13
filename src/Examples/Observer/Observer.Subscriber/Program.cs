using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Observer.Subscriber;
using Scabra.Observer.Subscriber.Hosting;

namespace Scabra.Examples.Observer
{
    class Program
    {
        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging
                .ClearProviders()
                .AddConsole();

            builder.Services
                .AddScabraObserverSubscriber(builder.Configuration)
                .AddSingleton(sp => {
                    var provider = sp.GetRequiredService<IScabraObserverSubscriberProvider>();
                    var subscriber = provider.GetSubscriber("publisher");
                    return new SomeClient(subscriber);
                });

            using var host = builder.Build();

            SomeClient client = null;

            var lifetimeService = host.Services.GetRequiredService<IHostApplicationLifetime>();

            lifetimeService.ApplicationStarted.Register(() => {
                client = host.Services.GetRequiredService<SomeClient>();
            });

            lifetimeService.ApplicationStopping.Register(() => {
                client?.Dispose();
            });

            host.Run();
        }
    }
}
