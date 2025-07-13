using NUnit.Framework;
using VerifyTests;

namespace Scabra.Rpc.Client
{
    [SetUpFixture]
    public class AssemblyInitializer
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            VerifySourceGenerators.Initialize();
        }
    }
}
