using System;

namespace Scabra.Examples.Docker
{
    public interface ISomeService
    {
        int ExecuteWithDelay(SomeCall call);

        string AcceptValues(int? i1, int? i2, TimeSpan? ts1, TimeSpan? ts2 = null, bool? b1 = null, bool? b2 = null);
    }
}
