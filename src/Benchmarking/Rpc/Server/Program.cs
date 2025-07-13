using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Server;
using Scabra.Rpc.Server;
using Scabra.Rpc.Server.Hosting;

namespace Scabra.Benchmarking.Rpc
{
    class Program
    {
        static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Logging
                .ClearProviders();

            builder.Services
                .AddScabraRpcServer(builder.Configuration);

            builder.Services.AddCodeFirstGrpc();

            using var app = builder.Build();

            var rpcServer = app.Services.GetRequiredService<IScabraRpcServer>();
            rpcServer.RegisterService<IBenchmarkableRpcService>(new BenchmarkableRpcService());

            app.MapGrpcService<BenchmarkableGoogleRpcService>();

            app.Run();
        }
    }
}
