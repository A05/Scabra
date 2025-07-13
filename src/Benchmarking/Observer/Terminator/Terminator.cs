namespace Scabra.Benchmarking.Observer
{
    internal class Terminator : ITerminator
    {
        public bool _isReady;

        public bool AreYouReady() => _isReady;
    }
}
