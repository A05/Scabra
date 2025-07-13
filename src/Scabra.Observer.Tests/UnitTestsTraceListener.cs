using NUnit.Framework;
using System.Diagnostics;

namespace Scabra.Observer
{
    public class UnitTestsTraceListener : DefaultTraceListener
    {
        public static void TurnOn()
        {
            Trace.Listeners.Clear();

            Trace.Listeners.Add(new UnitTestsTraceListener());
        }

        public override void Fail(string msg, string detailedMsg)
        {
            Assert.Fail($"{(string.IsNullOrEmpty(msg) ? "N/A" : msg)} : {(string.IsNullOrEmpty(detailedMsg) ? "N/A" : detailedMsg)}");
        }
    }
}
