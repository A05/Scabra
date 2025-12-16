using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Server.Hosting;
using Scabra.Rpc.Server;

namespace Scabra.Examples.k8s.Rpc
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
                .AddSingleton<PingPongService>()
                .AddScabraRpcServer(builder.Configuration);

            using var host = builder.Build();

            var rpcServer = host.Services.GetRequiredService<IScabraRpcServer>();
            var pingPongService = host.Services.GetRequiredService<PingPongService>();
            rpcServer.RegisterService<IPingPongService>(pingPongService);

            host.Run();
        }
    }
}
