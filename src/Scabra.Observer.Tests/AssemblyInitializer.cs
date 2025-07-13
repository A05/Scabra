using NUnit.Framework;

namespace Scabra.Observer
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
