using BenchmarkDotNet.Attributes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ProtoBuf.Grpc.Client;
using Scabra.Rpc.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scabra.Benchmarking.Rpc
{
    public class RpcBenchmark
    {
        private const int MARKS_ARRAY_LENGTH = 700;

        private IScabraRpcChannel _scabraRpcChannel;
        private IBenchmarkableRpcService _scabraService;
        private GrpcChannel _grpcChannel;
        private IBenchmarkableGoogleRpcService _grpcService;
        private int[] _marks1, _marks2, _marks3;
        private string _description1, _description2, _description3;

        [GlobalSetup]
        public void Setup() 
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            var address = configuration.GetSection("scabra:rpc:clients:0:address").Value;
            var scabraRpcChannelOptions = new ScabraRpcChannelOptions() { Address = address };

            _scabraRpcChannel = new ScabraRpcChannel(scabraRpcChannelOptions, NullLoggerFactory.Instance);
            _scabraService = new BenchmarkableRpcServiceProxy(_scabraRpcChannel);

            var gRpcServerUrl = configuration["gRpcServerUrl"];

            _grpcChannel = GrpcChannel.ForAddress(gRpcServerUrl);
            _grpcService = _grpcChannel.CreateGrpcService<IBenchmarkableGoogleRpcService>();

            _marks1 = new int[MARKS_ARRAY_LENGTH];
            for (int i = 0; i < _marks1.Length; i++) _marks1[i] = Random.Shared.Next(1000);

            _marks2 = new int[MARKS_ARRAY_LENGTH];
            for (int i = 0; i < _marks2.Length; i++) _marks2[i] = Random.Shared.Next(1000);
            
            _marks3 = new int[MARKS_ARRAY_LENGTH];
            for (int i = 0; i < _marks3.Length; i++) _marks3[i] = Random.Shared.Next(1000);

            _description1 = Encoding.Unicode.GetString(_marks1.Select(i => (byte)i).ToArray());
            _description2 = Encoding.Unicode.GetString(_marks2.Select(i => (byte)i).ToArray());
            _description3 = Encoding.Unicode.GetString(_marks3.Select(i => (byte)i).ToArray());

            // Wait for the server to start.

            do
                try 
                { 
                    _scabraService.NoParametersNoReturn(); break; 
                } 
                catch (Exception ex)
                { 
                    Console.WriteLine($"Server is not ready yet.\n{ex}"); 
                }
            while (true);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _scabraRpcChannel.Dispose();
            _grpcChannel.Dispose();
        }

        [Benchmark]
        public void No_Parameters_No_Return()
        {
            _scabraService.NoParametersNoReturn();
        }

        [Benchmark]
        public void Primitive_Parameters_Primitive_Return()
        {
            var primitive = _scabraService.PrimitiveParametersPrimitiveReturn(25, 48, 193);
        }

        [Benchmark]
        public void Complex_Parameters_Complex_Return()
        {
            var p1 = new ComplexEntity() { Id = 1, Name = "α name", Description = _description1, Marks = _marks1 };
            var p2 = new ComplexEntity() { Id = 2, Name = "γ name", Description = _description2, Marks = _marks2 };
            var p3 = new ComplexEntity() { Id = 3, Name = "γ name", Description = _description3, Marks = _marks3 };

            var complex = _scabraService.ComplexParametersComplexReturn(p1, p2, p3);
        }

        [Benchmark]
        public async Task No_Parameters_No_Return_gRpc()
        {
            await _grpcService.NoParametersNoReturn();
        }

        [Benchmark]
        public async Task Primitive_Parameters_Primitive_Return_gRpc()
        {
            PrimitiveCall call = new()
            {
                Parameter1 = 25,
                Parameter2 = 48,
                Parameter3 = 193
            };

            var reply = await _grpcService.PrimitiveParametersPrimitiveReturn(call);
        }

        [Benchmark]
        public async Task Complex_Parameters_Complex_Return_gRpc()
        {
            var p1 = new ComplexEntity() { Id = 1, Name = "α name", Description = _description1, Marks = _marks1 };
            var p2 = new ComplexEntity() { Id = 2, Name = "γ name", Description = _description2, Marks = _marks2 };
            var p3 = new ComplexEntity() { Id = 3, Name = "γ name", Description = _description3, Marks = _marks3 };

            ComplexCall call = new() { Parameter1 = p1, Parameter2 = p2, Parameter3 = p3 };

            var reply = await _grpcService.ComplexParametersComplexReturn(call);
        }
    }
}
