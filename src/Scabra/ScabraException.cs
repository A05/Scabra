using System;

namespace Scabra
{
    public class ScabraException : Exception
    {
        public ScabraException() : base()
        {
        }

        public ScabraException(string message) : base(message)
        {
        }

        public ScabraException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
