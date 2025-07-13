using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scabra.Rpc.Server;
using Scabra.Rpc.Server.Hosting;

namespace Scabra.Examples.Rpc.Security
{
    class Program
    {
        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging
                .ClearProviders()
                .AddConsole();

            var securityHandler = new ExampleScabraSecurityHandler(secritKey: "secret");

            builder.Services
                .AddScabraRpcServer(builder.Configuration, securityHandler)
                .AddSingleton<ISecurityService>(sp => new SecurityService(securityHandler))
                .AddSingleton<ISomeProtectedService, SomeProtectedService>();

            using var host = builder.Build();

            var rpcServer = host.Services.GetRequiredService<IScabraRpcServer>();

            var ss = host.Services.GetRequiredService<ISecurityService>();
            rpcServer.RegisterService(ss);

            var sps = host.Services.GetRequiredService<ISomeProtectedService>();
            rpcServer.RegisterService(sps);

            host.Run();
        }
    }
}
