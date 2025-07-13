using BenchmarkDotNet.Attributes;

namespace Scabra.Rpc
{
    public class RpcBenchmarks
    {
        private readonly RpcTests _test = new();

        [GlobalSetup]
        public void Setup()
        {
            _test.SetUp();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _test.TearDown();
        }

        [Benchmark]
        public void DoVoidJob()
        {
            _test.Proxy.DoVoidJob();
        }
        
        [Benchmark]
        public void DoPrimitiveJob()
        {
            _test.Proxy.DoPrimitiveJob(25);
        }
    }
}
