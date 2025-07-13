using BenchmarkDotNet.Configs;

namespace Scabra.Rpc
{
    public class DefaultBenchmarkConfig : ManualConfig
    {
        public DefaultBenchmarkConfig()
        {
            Add(DefaultConfig.Instance); // Use default settings

            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }
}
