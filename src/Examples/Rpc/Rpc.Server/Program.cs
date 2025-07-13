using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Server.Hosting;
using Scabra.Rpc.Server;

namespace Scabra.Examples.Rpc
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
                .AddScabraRpcServer(builder.Configuration);

            using var host = builder.Build();

            var rpcServer = host.Services.GetRequiredService<IScabraRpcServer>();
            rpcServer.RegisterService<ISomeService>(new SomeService());

            host.Run();
        }
    }
}
