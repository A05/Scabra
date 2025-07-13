using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using Scabra.Rpc;

[assembly: Config(typeof(DefaultBenchmarkConfig))]

namespace Scabra.Rpc
{
    [SetUpFixture]
    public class AssemblyInitializer
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            UnitTestsTraceListener.TurnOn();
        }
    }
}
