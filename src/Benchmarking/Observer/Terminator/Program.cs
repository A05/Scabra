using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Scabra.Benchmarking.Observer
{
    class Program
    {
        static void Main()
        {
            var summary = BenchmarkRunner.Run<ObserverBenchmark>();// new DebugInProcessConfig());
        }
    }
}
