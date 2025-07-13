using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Observer.Publisher.Hosting;

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
                .AddScabraObserverPublisher(builder.Configuration)
                .AddSingleton<SomeService>();

            using var host = builder.Build();

            SomeService service = null;

            var lifetimeService = host.Services.GetRequiredService<IHostApplicationLifetime>();

            lifetimeService.ApplicationStarted.Register(() => {
                service = host.Services.GetRequiredService<SomeService>();
            });

            lifetimeService.ApplicationStopping.Register(() => {
                service?.Dispose();
            });

            host.Run();
        }
    }
}
