using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Scabra.Benchmarking.Rpc
{
    class Program
    {
        static void Main()
        {
            var summary = BenchmarkRunner.Run<RpcBenchmark>(); // new DebugInProcessConfig());
        }
    }
}
