using System;

namespace Scabra.Examples.Rpc
{
    internal class SomeClient
    {
        private readonly ISomeService _service;

        public SomeClient(ISomeService service) 
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void DoRemoteProcedureCalls()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.Write($"Calling ({i}) ... ");

                var r = _service.DoAddition(i, 25);
                
                Console.WriteLine($"done! Return value is '{r}'.");
            }
        }
    }
}
