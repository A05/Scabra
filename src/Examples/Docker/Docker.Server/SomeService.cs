using System;
using System.Threading;

namespace Scabra.Examples.Docker
{
    public class SomeService : ISomeService
    {
        public int ExecuteWithDelay(SomeCall call)
        {
            var expExtraField = BitConverter.GetBytes(call.Delay);

            if (expExtraField.Length != call.ExtraField.Length)
                throw new ApplicationException($"Failed to pass the {nameof(SomeCall.ExtraField)} value.");

            for (int i = 0; i < call.ExtraField.Length; i++)
                if (expExtraField[i] != call.ExtraField[i])
                    throw new ApplicationException($"Failed to pass the {nameof(SomeCall.ExtraField)} value.");

            Thread.Sleep(call.Delay);

            if (call.Delay % 10 == 0)
                throw new ApplicationException($"{call.Delay}");

            return call.Delay;
        }

        public string AcceptValues(int? i1, int? i2, TimeSpan? ts1, TimeSpan? ts2 = null, bool? b1 = null, bool? b2 = null)
        {
            return $"{nameof(i1)}={i1}, {nameof(i2)}={i2}, {nameof(ts1)}={ts1}, {nameof(ts2)}={ts2}, {nameof(b1)}={b1}, {nameof(b2)}={b2}";
        }
    }
}
